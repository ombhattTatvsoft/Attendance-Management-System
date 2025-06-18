const today = new Date().toISOString().split('T')[0];
$(document).ready(function(){
    $("#startDate,#endDate").prop('max', today);
    $("#startDate").val(today);
    $("#endDate").val(today);
    $('#startDate').change(function () {
        $("#endDate").prop('min', $(this).val());
    });
    $('#endDate').change(function () {
        $("#startDate").prop('max', $(this).val());
    });
    fetchReport();
});
function resetReport(){
    $("#startDate").val(today);
    $("#endDate").val(today);
    fetchReport();
}
function fetchReport(){
    $.ajax({
        url: '/ReportAnalytics/FetchReport',
        type: 'POST',
        data: {
            startDate: $("#startDate").val(),
            endDate: $("#endDate").val()
        },
        success: function(response) {
            $("#reportTableContainer").html(response);
        }
    });
}