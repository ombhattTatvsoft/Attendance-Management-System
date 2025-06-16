const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance=[];
$(document).ready(function () {
    $("#teamMonth").val(currentMonth);
    if(teamUsers.length>0){ 
        getTeamAttendanceTable();
    }
    $(document).on('change','.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getTeamAttendanceTable() {
    $("#teamAttendanceHeaderRow").empty();
    $("#teamAttendanceBody").empty();
 
    teamUsers.forEach((element, index) => {
        $.ajax({
            type: "GET",
            url: "/Attendance/GetAttendanceTable",
            data: {
                month: $("#teamMonth").val(),
                year: $("#teamYear").val(),
                userId: element.id
            },
            success: function (data) {
                if (data.success === false) {
                    toastr.error("Failed loading attendance table");
                } else {
                    let wrapper = $('<div></div>').html(data);
 
                    if (index === 0) {
                        let header = wrapper.find("thead > tr").html();
                        $("#teamAttendanceHeaderRow").html(`<th>Name</th>${header}`);
                    }
 
                    let userRow = wrapper.find("tbody > tr").html();
                    let userName = element.name;
                    let summaryHtml = wrapper.find("h6").eq(1).html();
 
                    let attendanceRow = `<tr><td><strong>${userName}</strong></td>${userRow}</tr>`;
                    let summaryRow = `<tr><td colspan="${wrapper.find('thead > tr > th').length + 1}">${summaryHtml}</td></tr>`;
 
                    $("#teamAttendanceBody").append(summaryRow).append(attendanceRow);
                    if (!wrapper.find('td').hasClass('clickable')) {
                        $("#saveTeamAttendanceBtn").hide();
                    } else {
                        $("#saveTeamAttendanceBtn").show();
                    }
                }
            }
        });
    });
}
function resetTeamAttendanceTable() {
    $("#teamMonth").val(currentMonth);
    getTeamAttendanceTable();
}
function showDropdown(ele){
    $(ele).attr('data-status', $(ele).find('select').val());
    currentElement=ele;
}
function saveTeamAttendance(){
    var toBeChanged=$("#teamAttendanceTable select").filter(function() {return $(this).val()!=""});
    newAttendance = [];
    $(toBeChanged).each(function(){
        newAttendance.push({
            UserId:$(this).closest('td').data('userid'),
            Date:$(this).closest('td').data('date'),
            Status:$(this).val()
        });
    });
    if(newAttendance.length>0){
        var formData={
            UserId:currentUserId,
            Month:parseInt($("#teamMonth").val()),
            Year: parseInt($("#teamYear").val()),
            AttendanceRecords: newAttendance.map(record => ({
                UserId: record.UserId,
                Date: record.Date,
                Status: record.Status
            }))
        }
        $.ajax({
            type: "POST",
            url: "/Attendance/SaveAttendances",
            contentType:'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if(response.isSuccess){
                    toastr.success(response.message)
                    getTeamAttendanceTable();
                }
                else
                    toastr.error(response.message)
            }
        });
    }
}