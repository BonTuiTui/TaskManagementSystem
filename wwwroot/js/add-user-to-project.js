$(document).ready(function () {
    $('#addUserButton').on('click', function () {
        var projectId = $('#ProjectId').val();
        var userNameOrEmail = $('#userNameOrEmail').val();

        $.ajax({
            url: '/Projects/AddUserToProject',
            method: 'POST',
            data: {
                projectId: projectId,
                userName: userNameOrEmail
            },
            success: function (data) {
                var newMember = '<li>' + data.userName + ' (' + data.email + ')</li>';
                $('#projectMembersList').append(newMember);
                $('#addUserToProjectModal').modal('hide');
                $('#userNameOrEmail').val('');
            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });
    });
});