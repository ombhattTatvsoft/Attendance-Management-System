const today = new Date().toISOString().split('T')[0];

$(document).ready(function () {
    $('#startDate').change(function () {
        $("#endDate").prop('min', $(this).val());
    });
    $('#endDate').change(function () {
        $("#startDate").prop('max', $(this).val());
    });
    getLeaves();
    $(document).on('click','#addLeave,#editLeave',function () {
        $.ajax({
            url: '/Leave/UpsertLeave',
            data: { id: $(this).data('id') },
            type: 'GET',
            success: function (response) {
                $("#LeaveForm").html(response);
                $("#LeaveModal").modal('show');
                $("#fromDate,#toDate").prop('min', today);
            },
        });
    });
    $(document).on('change','#fromDate',function () {
        $("#toDate").prop('min', $('#fromDate').val());
    });
    $(document).on('change','#toDate',function () {
        $("#fromDate").prop('max', $('#toDate').val());
    });
    $(document).on('change','#fromDate,#toDate,#fromDateType,#toDateType',function () {
        if($('#fromDate').val() && $('#toDate').val()) {
            $.ajax({
                url: '/Leave/CountLeaveDays',
                data: { fromDate: $('#fromDate').val(), toDate: $('#toDate').val(),fromDateType: $('#fromDateType').val(), toDateType: $('#toDateType').val() },
                type: 'GET',
                success: function (response) {
                    $("#totalLeave").val(response.days);
                }
            });
        }
    });
    $("#LeaveForm").on("submit", function (e) {
        e.preventDefault();
        if ($(this).valid()) {
            $.ajax({
                url: '/Leave/UpsertLeave',
                type: 'POST',
                data: $(this).serialize(),
                success: function (response) {
                    $("#DepartmentModal").modal('hide');
                    if (response.success) {
                        location.reload();
                    } else {
                        toastr.error(response.message);
                    }
                }
            });
        }
    });
});
function refetchLeaves() {
    $("#leaveStatus").val('All');
    getLeaves();
}
function getLeaves() {
    $.ajax({
        url: '/Leave/GetLeaves',
        type: 'GET',
        data: {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val(),
            leaveStatus: $("#leaveStatus").val(),
            userId:currentUserId
        },
        success: function (response) {
            $("#leaveTableContainer").html(response);
            createDataTable("leaveTable");
        }
    });
}