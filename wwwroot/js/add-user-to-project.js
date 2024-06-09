$(document).ready(function () {
    // Load members when the modal is shown
    $('#updateProjectModal').on('show.bs.modal', function (event) {
        var projectId = $('#ProjectId').val();
        var currentMembersList = $('#currentMembersList');

        // Clear the existing list
        currentMembersList.empty();

        // Load project members
        $.ajax({
            url: '/Projects/GetProjectMembers',
            method: 'GET',
            data: { projectId: projectId },
            success: function (data) {
                data.forEach(function (member) {
                    var row = '<tr>' +
                        '<td>' + member.userName + '</td>' +
                        '<td>' + member.email + '</td>' +
                        '<td><button class="btn btn-danger btn-sm remove-member-button" data-user-id="' + member.id + '">Remove</button></td>' +
                        '</tr>';
                    currentMembersList.append(row);
                });
            },
            error: function () {
                console.error('Failed to load project members.');
            }
        });
    });

    // Add member to project
    $('#addUserButton').on('click', function () {
        var projectId = $('#ProjectId').val();
        var userNameOrEmail = $('#userNameOrEmail').val();

        $.ajax({
            url: '/Projects/AddUserToProject',
            method: 'POST',
            data: {
                projectId: projectId,
                userName: userNameOrEmail,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (data) {
                var row = '<tr>' +
                    '<td>' + data.userName + '</td>' +
                    '<td>' + data.email + '</td>' +
                    '<td><button class="btn btn-danger btn-sm remove-member-button" data-user-id="' + data.id + '">Remove</button></td>' +
                    '</tr>';
                $('#currentMembersList').append(row);
                $('#userNameOrEmail').val('');
            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });
    });

    // Remove member from project
    $(document).on('click', '.remove-member-button', function () {
        var projectId = $('#ProjectId').val();
        var userId = $(this).data('user-id');
        var listItem = $(this).closest('tr');

        $.ajax({
            url: '/Projects/RemoveUserFromProject',
            method: 'POST',
            data: {
                projectId: projectId,
                userId: userId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (data) {
                if (data.success) {
                    listItem.remove();
                } else {
                    alert(data.message);
                }
            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });
    });
});