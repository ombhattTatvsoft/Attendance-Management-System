$(document).ready(function () {
    $("#addDepartment,#editDepartment").click(function () {
        $.ajax({
            url: '/Department/UpsertDepartment',
            data: { id: $(this).data('id') },
            type: 'GET',
            success: function (response) {
                $("#DepartmentForm").html(response);
                $.validator.unobtrusive.parse($("#DepartmentForm"));
                $("#DepartmentModal").modal('show');
            },
        });
    });

    $("#DepartmentForm").on("submit", function (e) {
        e.preventDefault();
        if ($(this).valid()) {
            $.ajax({
                url: '/Department/UpsertDepartment',
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