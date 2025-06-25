const today = new Date().toISOString().split('T')[0];

$(document).ready(function () {
    $('#startDate').change(function () {
        $("#endDate").prop('min', $(this).val());
    });
    $('#endDate').change(function () {
        $("#startDate").prop('max', $(this).val());
    });
    $(document).on('click', '#addSelfLeave,#editSelfLeave,#viewSelfLeave,#editTeamLeave', function () {
        var id = $(this).data('id');
        var isView = $(this).attr('id') == 'viewSelfLeave';
        var teamEdit = $(this).attr('id') == 'editTeamLeave';
        $.ajax({
            url: '/Leave/UpsertLeave',
            data: { id: id },
            type: 'GET',
            success: function (response) {
                $("#LeaveForm").html(response);
                $("#teamLeaveForm").html(response);
                $.validator.unobtrusive.parse($("#LeaveForm,#teamLeaveForm"));
                $("#LeaveModal").modal('show');
                $("#fromDate,#toDate").prop('min', today);
                if (isView) {
                    $('#leaveFieldSet').attr('disabled', true);
                    $('#upsertBtn').hide();
                    $('.viewable').removeClass('d-none');
                }
                if (teamEdit) {
                    $('#upsertBtn').text('Save');
                    $('input,textarea').prop('readonly', true);
                    $('select').prop('disabled', true);
                    $('.viewable').removeClass('d-none');
                    $('.teamEditable').prop('readonly', false).prop('disabled', false);
                }
            },
        });
    });
    $(document).on('change', '#fromDate', function () {
        $("#toDate").prop('min', $('#fromDate').val());
    });
    $(document).on('change', '#toDate', function () {
        $("#fromDate").prop('max', $('#toDate').val());
    });
    $(document).on('change', '#fromDate,#toDate,#fromDateType,#toDateType', function () {
        if($("#fromDate").val() == $("#toDate").val()) {
            if($(this).attr('id') === 'toDateType')
                $("#fromDateType").val($("#toDateType").val());
            else
            $("#toDateType").val($("#fromDateType").val());
        }
        if ($('#fromDate').val() && $('#toDate').val()) {
            $.ajax({
                url: '/Leave/CountLeaveDays',
                data: { fromDate: $('#fromDate').val(), toDate: $('#toDate').val(), fromDateType: $('#fromDateType').val(), toDateType: $('#toDateType').val() },
                type: 'GET',
                success: function (response) {
                    $("#totalLeave").val(response.days);
                }
            });
        }
    });
    $("#LeaveForm,#teamLeaveForm").on("submit", function (e) {
        var isTeamForm = $(this).attr('id') === 'teamLeaveForm';
        e.preventDefault();
        if ($(this).valid()) {
            $.ajax({
                url: '/Leave/UpsertLeave',
                type: 'POST',
                data: $(this).serialize(),
                success: function (response) {
                    $("#LeaveModal").modal('hide');
                    if (response.success) {
                        toastr.success(response.message);
                        if (isTeamForm)
                            getLeaves(true);
                        else
                            getLeaves();
                    } else {
                        toastr.error(response.message);
                    }
                }
            });
        }
    });
});
function refetchLeaves(team) {
    $("#startDate").val("");
    $("#endDate").val("");
    $("#startDate,#endDate").removeAttr('min');
    $("#leaveStatus").val('All');
    getLeaves(team);
}
function getLeaves(team) {
    $.ajax({
        url: '/Leave/GetLeaves',
        type: 'GET',
        data: {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            leaveStatus: $("#leaveStatus").val(),
            userId: currentUserId,
            team: team
        },
        success: function (response) {
            $("#leaveTableContainer").html(response);
            createDataTable("leaveTable");
            if (team) {
                $('.notTeamBtn').hide();
            }
        }
    });
}
function cancelLeave(id) {
    document.getElementById("deleteTitle").innerText = "leave";
    $('#deleteConfirm').click(function () {
        $.ajax({
            url: '/Leave/CancelLeave',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    getLeaves();
                } else {
                    toastr.error(response.message);
                }
            }
        });
        $("#deleteModal").modal('hide');
    })
}
