document.addEventListener('DOMContentLoaded', function () {
    var deleteButtons = document.querySelectorAll('.delete-task-button');
    var confirmDeleteButton = document.getElementById('confirmDeleteButton');
    var confirmDeleteTaskModal = new bootstrap.Modal(document.getElementById('confirmDeleteTaskModal'));

    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            var taskId = this.getAttribute('data-task-id');
            confirmDeleteTaskModal.show();

            confirmDeleteButton.addEventListener('click', function () {
                fetch('/Tasks/Delete/' + taskId, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }).then(response => {
                    if (response.ok || response.status === 200 || response.statusText.toLowerCase() === 'ok') {
                        confirmDeleteTaskModal.hide(); // Ẩn modal sau khi xóa thành công
                        location.reload(); // Reload the page after deleting the task

                    } else {
                        console.error('Error deleting task');
                    }
                }).catch(error => {
                    console.error('Error:', error);
                });
            });
        });
    });
});