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

function enableInteract() {
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

function userInteractIdea(ideaId) {

    // disable button. enable it untils the connection is started.
    disableInteract();

    var connection = new signalR
        .HubConnectionBuilder()
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .withUrl("/ideaInteractHub")
        .build();

    connection.on("IdeaStatus", (res) => {
        isReacted = res.isReacted;
        isCommented = res.isCommented;
        console.log(res);
        enableInteract();
        $('#countthumbup').html(res.thumbUp);
        $('#countthumbdown').html(res.thumbDown);
        $('#countcomment').html(res.numComment);
        $('#countview').html(res.numView);
    });

    connection.on("ResponseUserIdeaReaction", (res) => {
        //console.log(res);

        reactType = res.react;

        if (res.ideaId == ideaId) {
            setUserReaction(res.react);
        }
    });

    connection.on("ReceiveComment", (res) => {

        console.log(res);
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

    connection.onreconnecting((er) => {
        console.log(er);
        disableInteract();
    });
    connection.onreconnected((er) => {
        console.log(er);
        enableInteract();
    });

    connection.start().then(function () {
        enableInteract();
        connection.invoke("RegisterIdeaStatus", ideaId);
    }).catch(function (er) {
        console.log(er);
    });

    $('#react-form input').click(function (event) {
        //console.log(event.target.id);

        var isChecked = event.target.checked;
        $('#react-form input').prop('checked', false);
        event.target.checked = isChecked;

        if (isChecked) {
            console.log('update')

            connection.invoke("ReactIdea", {
                ideaId: ideaId,
                type: 'update',
                newreact: event.target.id
            });

        }
        else {
            console.log('delete')
            connection.invoke("ReactIdea", {
                ideaId: ideaId,
                type: 'delete',
            });
        }
        $(`label[for=${event.target.id}] span`).html('Loading...');
    });

    $('#comment-form').submit(function (event) {
        var comment = $('#comment').val();

        //$('#comment').val("");

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
        }).then(response => {
            return response.text();
        }).then((data) => {
            console.log(data);
        }).catch((er) => {
            console.log(er);
        });

        $('#comment-form')[0].reset();

        event.preventDefault();
    });
}

function deleteComment(cmtId) {
    //console.log("clicked delete btn");

    fetch(`/Forum/Comment/Delete/${cmtId}`, {
        method: 'DELETE'
    }).then((res) => {
        //console.log(res);
        if (res.ok) {
            $(`#cmt-${cmtId}`).remove();
        }
        return res.text();
    }).then((data) => {
        $('#countcomment').html(data)
    })
}