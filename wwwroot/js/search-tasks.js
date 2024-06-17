$(document).ready(function () {
    var projectId = window.projectId;

    // Lấy danh sách thành viên của dự án
    $.ajax({
        url: '/Members/GetProjectMembers?projectId=' + projectId,
        type: 'GET',
        success: function (members) {
            var assigneeSelect = $('#taskAssigneeSearchInput');
            members.forEach(function (member) {
                var option = '<option value="' + member.userName + '">' + member.userName + '</option>';
                assigneeSelect.append(option);
            });
        },
        error: function () {
            console.error('Failed to fetch project members');
        }
    });

    // Xử lý sự kiện tìm kiếm
    $('#searchButton').on('click', function () {
        var title = $('#taskTitleSearchInput').val();
        var status = $('#taskStatusSearchInput').val();
        var assignee = $('#taskAssigneeSearchInput').val();
        var dueDate = $('#taskDueDateSearchInput').val();

        $.ajax({
            url: '/Tasks/AdvancedSearchTasks',
            type: 'GET',
            data: {
                title: title,
                status: status,
                assignee: assignee,
                dueDate: dueDate
            },
            success: function (tasks) {
                $('#advancedSearchResults').empty();
                if (tasks.length > 0) {
                    tasks.forEach(function (task) {
                        var resultHtml = '<div class="search-result" onclick="openTaskModal(' + task.task_id + ')">' +
                            '<h5>' + task.title + '</h5>' +
                            '<p>' + task.description + '</p>' +
                            '</a>' +
                            '</div>';
                        $('#advancedSearchResults').append(resultHtml);
                    });
                } else {
                    $('#advancedSearchResults').append('<p>No tasks found</p>');
                }
            },
            error: function () {
                $('#advancedSearchResults').empty();
                $('#advancedSearchResults').append('<p>Failed to fetch tasks</p>');
            }
        });
    });
});