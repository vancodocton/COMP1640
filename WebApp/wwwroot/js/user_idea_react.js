const ideaId = 5;

// disable button. enable it untils the connection is started.
function disableReact() {
    document.getElementById("ThumbUp").disabled = true;
    document.getElementById("ThumbDown").disabled = true;
}

function enableReact() {
    document.getElementById("ThumbUp").disabled = false;
    document.getElementById("ThumbDown").disabled = false;
}

disableReact();

var connection = new signalR
    .HubConnectionBuilder()
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .withUrl("/ideaReactHub")
    .build();

connection.on("IdeaReactStatus", (res) => {
    $('#countthumbup').html(res.thumbUp);
    $('#countthumbdown').html(res.thumbDown);
});

connection.on("ReactIdeaResponse", (res) => {
    // reset
    console.log(res);
    if (res.ideaId == ideaId) {
        $(`#react-Model input`).prop('checked', false)
        var status = $(`#react-Model span`);
        var unchecked = $(`#react-Model input`);

        for (var i = 0; i < status.length; i++) {
            status[i].innerText = unchecked[i].getAttribute('data-unchecked');
        }

        // load again
        if (res.react != null) {
            $(`#${res.react}`).prop('checked', true);
            var checkedstatus = $(`#${res.react}`).data('checked');
            $(`label[for=${res.react}] span`).text(checkedstatus);
        }
    }

});

connection.onreconnecting((er) => {
    disableReact();
});

connection.onreconnected((er) => {
    enableReact();
});

connection.start().then(function () {
    enableReact();

    connection.invoke("RegisterIdeaReactStatus", ideaId);
});


$('#react-Model input').click(function (event) {
    console.log(event.target.id);

    var isChecked = event.target.checked;

    $('#react-Model input').prop('checked', false);

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
