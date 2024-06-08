$(document).ready(function () {
    $('#viewMembersModal').on('show.bs.modal', function () {
        var projectId = $('#ProjectId').val();
        var membersList = $('#membersList');

        // Clear the existing list
        membersList.empty();

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
                        '<td>' + member.fullName + '</td>' +
                        '</tr>';
                    membersList.append(row);
                });
            },
            error: function () {
                console.error('Failed to load project members.');
            }
        });
    });
});