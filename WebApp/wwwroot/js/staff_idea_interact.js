"use strict";

var isReacted = true;
var isCommented = true;
var reactType = null;
var userName = null;

function disableInteract() {
    document.getElementById("ThumbUp").disabled = true;
    document.getElementById("ThumbDown").disabled = true;
    document.getElementById("sendComment").disabled = true;
    document.getElementById("comment").disabled = true;
}

function enableInteract(isReacted, isCommented) {
    document.getElementById("ThumbUp").disabled = !isReacted;
    document.getElementById("ThumbDown").disabled = !isReacted;
    document.getElementById("sendComment").disabled = !isCommented;
    document.getElementById("comment").disabled = !isCommented;
}

function setUserReaction(reactType) {
    // reset
    $(`#react-form input`).prop('checked', false)
    // load again
    if (reactType != null) {
        $(`#${reactType}`).prop('checked', true);
    }
}

// disable button. enable it untils the connection is started.
disableInteract();

function userInteractIdea(ideaId) {
    var connection = new signalR
        .HubConnectionBuilder()
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .withUrl(`/ideaInteractHub?IdeaId=${ideaId}`)
        .build();

    connection.onclose((er) => {
        disableInteract();
    });

    connection.onreconnecting((er) => {
        disableInteract();
    });

    connection.on("ReceiveRegisteredConfirmation", function (per, reaction) {
        //console.log(reaction);
        //console.log(per);
        setUserReaction(reaction.reactType);
        isReacted = per.isReacted;
        isCommented = per.isCommented;

        enableInteract(isReacted, isCommented);
    });

    connection.on("ReceiveInteractionStatus", (res) => {
        //console.log(res);
        $('#countthumbup').html(res.thumbUp);
        $('#countthumbdown').html(res.thumbDown);
        $('#countcomment').html(res.numComment);
        $('#countview').html(res.numView);
    });

    connection.on("ReceiveReaction", (res) => {
        //console.log(res);
        reactType = res.reactType;

        if (res.userName == userName && res.ideaId == ideaId) {
            setUserReaction(res.reactType);
        }
    });

    connection.on("ReceiveComment", (res) => {
        //console.log(res);
        var cmt;
        if (userName != res.userName) {
            cmt =
                `
                <li class="card mb-1">
                    <div class="card-body bg-light">
                        <p>${res.content}</p>
                            <div class="d-flex justify-content-between">
                                <div class="d-flex flex-row align-items-center">
                                    <span class="fst-italic">Written by &nbsp;</span>
                                    <i class="fa-solid fa-user"></i>
                                    <p class="small mb-0 ms-2"><b>${res.userName}</b></p>
                                </div>
                            </div>
                    </div>
                 </li>
                `;
        }
        else {
            cmt =
                `
                <li id="cmt-${res.commentId}" class="card mb-1">
                    <div class="card-body bg-light">
                        <p>${res.content}</p>
                            <div class="d-flex justify-content-between">
                                <div class="d-flex flex-row align-items-center">
                                    <span class="fst-italic">Written by &nbsp;</span>
                                    <i class="fa-solid fa-user"></i>
                                    <p class="small mb-0 ms-2"><b>${res.userName}</b></p>
                                    <button class="btn btn-sm btn-white text-danger rounded-3 px-2"
                                        onclick="deleteComment(${res.commentId})">
                                        <i class="fa-solid fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                    </div>
                 </li>
                `;
        }

        $('#commentsList').append(cmt);
    });

    connection.on("RevokeSentComment", (res) => {
        //console.log(res);
        var revokedCmtId = res.commentId;
        var revokerUserName = res.revokerUserName;
        var ownerUsername = res.commentOwnerUserName;

        var cmtBody = $(`#cmt-${revokedCmtId} .card-body`)[0];
        if (cmtBody != undefined) {
            if (revokerUserName == ownerUsername)
                if (ownerUsername == userName)
                    cmtBody.innerHTML = `Your comment is revoked by yourself.`;
                else
                    cmtBody.innerHTML = `<i class="fa-solid fa-user"></i> <b>${ownerUsername}</b> revoked this own comment`;
            else
                if (ownerUsername == userName)
                    cmtBody.innerHTML = `Your comment is revoked by <i class="fa-solid fa-user"></i> <b>${revokerUserName}</b>`;
                else
                    if (revokerUserName == userName)
                        cmtBody.innerHTML = `This comment of <i class="fa-solid fa-user"></i> <b>${ownerUsername}</b> is revoked by yourself`;
                    else
                        cmtBody.innerHTML = `This comment of <i class="fa-solid fa-user"></i> <b>${ownerUsername}</b> is revoked by <i class="fa-solid fa-user"></i> <b>${revokerUserName}</b>`;
        }
    });

    async function start() {
        try {
            await connection.start();
            console.assert(connection.state === signalR.HubConnectionState.Connected);
            console.log("SignalR Connected.");
        } catch (err) {
            console.assert(connection.state === signalR.HubConnectionState.Disconnected);
            console.log(err);
            setTimeout(() => start(), 1000);
        }
    };

    start();

    $('#react-form input').click(function (event) {
        //console.log(event.target.id);

        var isChecked = event.target.checked;
        $('#react-form input').prop('checked', false);

        event.target.checked = isChecked;

        if (isChecked) {
            //console.log('update reaction')
            connection.invoke("ReactIdea", {
                ideaId: ideaId,
                reactType: event.target.id
            });
        }
        else {
            //console.log('delete reaction')
            connection.invoke("ReactIdea", {
                ideaId: ideaId,
                reactType: null,
            });
        }

        var spinner = `
            <div class="spinner-border spinner-border-sm" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>`;
        $(`label[for=${event.target.id}] span`).html(spinner);
    });
}

$('#comment-form').submit(function (event) {
    var comment = $('#comment').val();
    document.getElementById("sendComment").disabled = true;
    var comment = {
        ideaId: ideaId,
        content: comment
    }
    fetch("/Forum/Comment/Add", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(comment)
    }).then(res => {
        if (res.ok) {
            res.text().then(txt => console.log(`Added cmt ${txt}`));
            $('#comment-form')[0].reset();
            document.getElementById("sendComment").disabled = false;
        }
        return res;
    }).catch((er) => {
        console.log(er);
    });

    event.preventDefault();
});

function deleteComment(cmtId) {
    fetch(`/Forum/Comment/Delete/${cmtId}`, {
        method: 'DELETE'
    }).then((res) => {
        //console.log(res);
        if (res.ok) {
            //$(`#cmt-${cmtId}`).remove();
            res.text().then(txt => {
                $('#countcomment').html(txt);
            });
        }
        return res;
    }).catch(er => {
        console.log(er);
    })
}