const currentMonth = new Date().getMonth() + 1;
var currentElement;
var newAttendance=[];
$(document).ready(function () {
    if(currentRoleId!=1){
        $("#myMonth").val(currentMonth);
        getMyAttendanceTable(currentUserId);
    }
    if(teamUsers.length>0){ 
        $("#teamMonth").val(currentMonth);
        getTeamAttendanceTable();
    }
    $(document).on('change','.attendance-table-wrapper select', function () {
        $(currentElement).attr('data-status', $(this).val());
        $(this).removeClass('today-not-filled');
    });
});
function getMyAttendanceTable(id) {
    $.ajax({
        type: "GET",
        url: "/Attendance/GetAttendanceTable",
        data: {
            month: $("#myMonth").val(),
            year: $("#myYear").val(),
            userId:id
        },
        success: function (data) {
            if (data.success == false)
                toastr.error("Failed loading attendance table")
            else{
                $("#myAttendanceTable").html(data);
                if(!$("#myAttendanceTable td").hasClass('clickable')){
                    $("#saveMyAttendanceBtn").hide()
                }
            }
        }
    });
}
function getTeamAttendanceTable(){
    $("#teamAttendanceTable").empty();
    teamUsers.forEach(element => {
        $.ajax({
            type: "GET",
            url: "/Attendance/GetAttendanceTable",
            data: {
                month: $("#teamMonth").val(),
                year: $("#teamYear").val(),
                userId:element.id
            },
            success: function (data) {
                if (data.success == false)
                    toastr.error("Failed loading attendance table")
                else{
                    let wrapper=$('<div></div>').html(data);
                    wrapper.find('.user-title').text(element.name);
                    $("#teamAttendanceTable").append(wrapper.html()); 
                    if(!$("#teamAttendanceTable td").hasClass('clickable')){
                        $("#saveTeamAttendanceBtn").hide()
                    }
                    else{
                        $("#saveTeamAttendanceBtn").show()
                    }
                }
            }
        });
    });
}

function resetMyAttendanceTable(id) {
    $("#myMonth").val(currentMonth);
    getMyAttendanceTable(id);
}
function resetTeamAttendanceTable() {
    $("#teamMonth").val(currentMonth);
    getTeamAttendanceTable();
}
function showDropdown(ele){
    $(ele).attr('data-status', $(ele).find('select ').val());
    currentElement=ele;
}
function saveMyAttendance(){
    newAttendance = [];
    $("#myAttendanceTable [data-status]").each(function(){
        newAttendance.push({
            UserId:$(this).data('userid'),
            Date:$(this).data('date'),
            Status:$(this).data('status')
        });
    });
    if(newAttendance.length>0){
        var formData={
            UserId:newAttendance[0].UserId,
            Month:parseInt($("#myMonth").val()),
            Year: parseInt($("#myYear").val()),
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
                    getMyAttendanceTable(currentUserId);
                }
                else
                    toastr.error(response.message)
            }
        });
    }
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