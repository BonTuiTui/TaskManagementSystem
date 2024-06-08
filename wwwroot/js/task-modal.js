document.addEventListener('DOMContentLoaded', function () {
    var taskModal = document.getElementById('taskModal');
    taskModal.addEventListener('show.bs.modal', function (event) {
        var button = event.relatedTarget;
        var taskId = button.getAttribute('data-task-id');
        var status = button.getAttribute('data-status');

        console.log("Test status here " + status);

        if (taskId) {
            fetch('/Tasks/GetTask/' + taskId)
                .then(response => response.json())
                .then(task => {
                    document.getElementById('TaskId').value = task.task_id;
                    document.getElementById('Title').value = task.title;
                    document.getElementById('TaskDescription').value = task.description;
                    document.getElementById('AssignedTo').value = task.assignedToUsername;
                    document.getElementById('DueDate').value = task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : '';
                    document.getElementById('Status').value = task.status;
                    document.getElementById('taskModalLabel').innerText = 'Edit Task';
                });
        } else {
            // Reset form fields
            document.getElementById('taskForm').reset();
            document.getElementById('TaskId').value = '';
            document.getElementById('Status').value = status; // Set status to the passed value
            document.getElementById('taskModalLabel').innerText = 'Add Task';
            document.getElementById('DueDate').value = new Date().toISOString().split('T')[0];
        }
    });

    document.getElementById('saveTaskButton').addEventListener('click', function () {
        var form = document.getElementById('taskForm');
        var formData = new FormData(form);
        var taskId = document.getElementById('TaskId').value;
        var url = taskId ? '/Tasks/Edit/' + taskId : '/Tasks/Add';

        fetch(url, {
            method: 'POST',
            headers: {
                'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: formData
        }).then(response => {
            if (response.ok) {
                location.reload();
            } else {
                console.error('Error saving task');
            }
        }).catch(error => {
            console.error('Error:', error);
        });
    });
});