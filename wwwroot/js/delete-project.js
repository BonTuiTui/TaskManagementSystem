document.addEventListener('DOMContentLoaded', function () {
    var deleteButtons = document.querySelectorAll('.delete-project-button');
    var confirmDeleteProjectButton = document.getElementById('confirmDeleteProjectButton');
    var confirmDeleteProjectModal = new bootstrap.Modal(document.getElementById('confirmDeleteProjectModal'));

    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            var projectId = button.getAttribute('data-project-id'); // Lấy Project ID từ thuộc tính data

            console.log("Project ID to delete: " + projectId);

            confirmDeleteProjectModal.show();

            confirmDeleteProjectButton.addEventListener('click', function () {
                fetch('/Projects/Delete/' + projectId, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': '@antiforgeryTokenSet.RequestToken' // Thêm token chống CSRF
                    }
                }).then(response => {
                    if (response.ok || response.status === 200 || response.statusText.toLowerCase() === 'ok') {
                        confirmDeleteProjectModal.hide(); // Ẩn modal sau khi xóa thành công
                        window.location.href = '/'; // Điều hướng về trang index
                    } else {
                        console.error('Error deleting project');
                    }
                }).catch(error => {
                    console.error('Error:', error);
                });
            }, { once: true }); // Sử dụng { once: true } để đảm bảo sự kiện chỉ xảy ra một lần
        });
    });
});