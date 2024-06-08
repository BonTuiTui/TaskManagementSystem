$(document).ready(function () {
    // Lấy taskId từ URL
    function getParameterByName(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    // Sử dụng hàm để lấy giá trị của taskId từ URL
    var taskIdFromUrl = getParameterByName('taskId');
    console.log("getParameterByName = " + taskIdFromUrl);

    // Kiểm tra xem taskId có giá trị không và gọi hàm để load comments và hiển thị modal
    if (taskIdFromUrl) {
        loadTaskCommentsAndShowModal(taskIdFromUrl);
    }

    // Function to load comments for a specific task and show modal
    function loadTaskCommentsAndShowModal(taskId) {
        console.log("Loading comments for task ID: " + taskId);
        $.get('/TaskComments/GetTaskComments', { taskId: taskId }, function (data) {
            console.log(data); // Add this line to inspect the response

            // Sắp xếp dữ liệu theo thời gian (tăng dần hoặc giảm dần)
            data.sort(function (a, b) {
                return new Date(b.createAt) - new Date(a.createAt); // Sắp xếp giảm dần theo thời gian
            });

            $('#taskCommentList').empty();
            if (data.length === 0) {
                $('#taskCommentList').append('<p>No comments available</p>');
            } else {
                data.forEach(function (comment) {
                    $('#taskCommentList').append('<p>' + new Date(comment.createAt).toLocaleString() + ' - ' + comment.userName + ': ' + comment.comment_text + '</p>');
                });
            }
            $('#taskCommentModal').modal('show'); // Show modal after loading comments
        }).fail(function () {
            console.error('Failed to load comments');
        });
    }

    // Event listener for opening the comment modal
    $('.btn-comment').click(function () {
        var taskId = $(this).closest('.card-body').data('task-id');
        console.log("Button clicked, taskId: " + taskId);
        $('#TaskId').val(taskId);
        loadTaskCommentsAndShowModal(taskId); // Call the new function to load comments and show modal
    });

    // Event listener for saving a new comment
    $('#saveCommentButton').click(function () {
        var taskId = $('#TaskId').val();
        if (!taskId) {
            taskId = getParameterByName('taskId');
        }
        console.log("Task ID for saving comment:", taskId);

        var taskComment = {
            Task_id: taskId,
            Comment_text: $('#comment').val(),
            CreateAt: new Date().toISOString(),
            User_id: $('#UserId').val()  // Assuming UserId is stored in a hidden field
        };

        console.log("Saving comment:", taskComment);

        $.post('/TaskComments/AddTaskComment', taskComment, function () {
            loadTaskCommentsAndShowModal(taskComment.Task_id);
            $('#comment').val('');
        }).fail(function (response) {
            console.error('Failed to add comment:', response.responseText);
            alert('Failed to add comment');
        });
    });
});