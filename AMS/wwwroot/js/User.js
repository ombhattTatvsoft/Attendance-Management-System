
$(document).ready(function(){
    $("#roleId").change(function () {
        console.log("hello")
        if ($(this).val() == "1") {
            $("#managerId").parent().parent().removeClass("d-none");
        } else {
            $("#managerId").parent().parent().addClass("d-none");
        }
    });
    $("#managerId").focus(function(){
        if($("#department").val() == "") {
            toastr.warning("Please select department first");
        }
    });
});
