$(document).ready(function () {
    var currentUserName = '';

    window.openTaskModal = function (taskId) {
        $('#TaskId').val(taskId); // Thiết lập taskId trong modal
        $('comment').empty();
        $.ajax({
            url: '/Tasks/GetTask/' + taskId,
            type: 'GET',
            success: function (task) {
                console.log(task); // Kiểm tra dữ liệu task nhận được

                // Lưu tên người dùng hiện tại (giả sử thông tin người dùng hiện tại được trả về từ API)
                currentUserName = task.currentUsername; // Giả sử 'currentUserName' là trường trong response

                // Cập nhật badge status với màu tương ứng
                var statusBadge = $('#task-status');
                statusBadge.removeClass('bg-warning bg-primary bg-success bg-secondary');

                switch (task.status) {
                    case 'To Do':
                        statusBadge.addClass('bg-secondary text-dark').text('To Do');
                        break;
                    case 'In Progress':
                        statusBadge.addClass('bg-primary text-white').text('In Progress');
                        break;
                    case 'Need Review':
                        statusBadge.addClass('bg-warning text-dark').text('Need Review');
                        break;
                    case 'Done':
                        statusBadge.addClass('bg-success text-white').text('Done');
                        break;
                    default:
                        statusBadge.addClass('bg-secondary text-dark').text(task.Status);
                }

                $('#detailtaskmodalLabel').text(task.title);
                $('#task-assignee').text(task.assignedUser);

                // Định dạng ngày tháng năm rõ ràng
                var dueDate = new Date(task.dueDate);
                if (!isNaN(dueDate)) {
                    var options = { day: '2-digit', month: 'long', year: 'numeric' };
                    $('#task-due-date').text(dueDate.toLocaleDateString('en-GB', options));
                } else {
                    $('#task-due-date').text('Invalid Date');
                }

                $('#task-description').text(task.description);

                // Lấy danh sách comment
                $.get('/TaskComments/GetTaskComments', { taskId: taskId }, function (data) {
                    console.log(data); // Inspect the response

                    // Sắp xếp dữ liệu theo thời gian (tăng dần hoặc giảm dần)
                    data.sort(function (a, b) {
                        return new Date(b.createAt) - new Date(a.createAt); // Sắp xếp giảm dần theo thời gian
                    });

                    $('#taskCommentList').empty();
                    if (data.length === 0) {
                        $('#taskCommentList').append('<p id="no-comments">No comments available</p>');
                    } else {
                        data.forEach(function (comment) {
                            var commentTime = formatTimeAgo(new Date(comment.createAt));
                            var commentHtml = '<div class="activity-comment">' +
                                '<div>' +
                                '<p><strong>' + comment.userName + '</strong> &mdash; ' + commentTime + '</p>' +
                                '<p>' + comment.comment_text + '</p>' +
                                '</div>' +
                                '</div>';
                            $('#taskCommentList').append(commentHtml);
                        });
                    }
                }).fail(function () {
                    console.error('Failed to load comments');
                });

                $('#detailtaskmodal').modal('show'); // Đảm bảo modal được kích hoạt
            },
            error: function () {
                alert('Failed to fetch task details.');
            }
        });
    }

    function formatTimeAgo(date) {
        const now = new Date();
        const secondsAgo = Math.floor((now - date) / 1000);

        const intervals = {
            year: 31536000,
            month: 2592000,
            week: 604800,
            day: 86400,
            hour: 3600,
            minute: 60,
            second: 1
        };

        for (const [unit, value] of Object.entries(intervals)) {
            const interval = Math.floor(secondsAgo / value);
            if (interval >= 1) {
                return interval === 1 ? `1 ${unit} ago` : `${interval} ${unit}s ago`;
            }
        }

        return 'Just now';
    }

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
            var commentTime = formatTimeAgo(new Date(taskComment.CreateAt));
            var commentHtml = '<div class="activity-comment">' +
                '<div>' +
                '<p><strong>' + currentUserName + '</strong> &mdash; ' + commentTime + '</p>' +
                '<p>' + taskComment.Comment_text + '</p>' +
                '</div>' +
                '</div>';

            // Remove "No comments available" message if it exists
            $('#no-comments').remove();

            $('#taskCommentList').prepend(commentHtml); // Add new comment to the top of the list
            $('#comment').val('');
        }).fail(function (response) {
            console.error('Failed to add comment:', response.responseText);
            alert('Failed to add comment');
        });
    });

    // Xóa form khi mở modal
    $('#detailtaskmodal').on('show.bs.modal', function () {
        $('#comment').val('');
    });

    // Helper function to get URL parameters
    function getParameterByName(name, url = window.location.href) {
        name = name.replace(/[\[\]]/g, '\\$&');
        var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    // Kiểm tra URL khi trang được tải và mở modal nếu có taskId
    var taskIdFromUrl = getParameterByName('taskId');
    console.log("getParameterByName = " + taskIdFromUrl);

    if (taskIdFromUrl) {
        openTaskModal(taskIdFromUrl);
    }
});