﻿document.addEventListener('DOMContentLoaded', function () {
    var todoColumn = document.getElementById('todoColumn');
    var inProgressColumn = document.getElementById('inProgressColumn');
    var needReviewColumn = document.getElementById('needReviewColumn');
    var doneColumn = document.getElementById('doneColumn');
    var csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    [todoColumn, inProgressColumn, doneColumn, needReviewColumn].forEach(function (column) {
        new Sortable(column, {
            group: 'shared',
            animation: 150,
            onEnd: function (evt) {
                console.log('Drag Ended:', evt);
                var taskId = evt.item.getAttribute('data-task-id');
                var newStatus;
                if (evt.to.id === 'todoColumn') {
                    newStatus = 'To Do';
                } else if (evt.to.id === 'inProgressColumn') {
                    newStatus = 'In Progress';
                } else if (evt.to.id === 'needReviewColumn') {
                    newStatus = 'Need Review';
                } else if (evt.to.id === 'doneColumn') {
                    newStatus = 'Done';
                }

                console.log('Task ID:', taskId, 'New Status:', newStatus);

                fetch('/Tasks/UpdateStatus', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': csrfToken
                    },
                    body: JSON.stringify({ TaskId: taskId, Status: newStatus })
                }).then(response => {
                    if (response.ok) {
                        var card = evt.item;
                        var updatedAtElement = card.querySelector('.updated-at');
                        var updatedAt = new Date().toLocaleString().replace(',', ''); // Remove comma
                        updatedAtElement.innerText = updatedAt;

                        updateTaskCounts();
                    } else {
                        console.error('Error updating task status');
                    }
                }).catch(error => {
                    console.error('Error:', error);
                });
            }
        });
    });

    var updateTaskCounts = function () {
        var todoCount = todoColumn.children.length;
        var inProgressCount = inProgressColumn.children.length;
        var needReviewCount = needReviewColumn.children.length;
        var doneCount = doneColumn.children.length;

        document.getElementById('todoCountHeader').textContent = `To Do (${todoCount})`;
        document.getElementById('inProgressCountHeader').textContent = `In Progress (${inProgressCount})`;
        document.getElementById('needReviewCountHeader').textContent = `Need Review (${needReviewCount})`;
        document.getElementById('doneCountHeader').textContent = `Done (${doneCount})`;
    };

    updateTaskCounts();
});