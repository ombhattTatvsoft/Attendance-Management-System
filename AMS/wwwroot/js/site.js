// toggle sidebar
function toggleSidebar() {
    var sidebar = document.getElementById("mySidebar");
    sidebar.classList.toggle("hidden");
    sidebar.classList.toggle("visible");
    $("#mainBody").toggleClass("main-margin");
}
// password visibility
function togglePassword(id1,id2){
    const pwd=document.getElementById(id1);
    const eyeicon=document.getElementById(id2);
    if(pwd.getAttribute("type")=== "password"){
        pwd.setAttribute("type",'text');
        eyeicon.classList.remove('fa-eye-slash');
        eyeicon.classList.add('fa-eye');
    }
    else{
        pwd.setAttribute("type",'password');
        eyeicon.classList.add('fa-eye-slash');
        eyeicon.classList.remove('fa-eye');
    }
}
// toggle attendance sub menu
function toggleSubMenu(id) {
    const menu = document.getElementById(id);
    menu.classList.toggle("d-none");
}
// create data table
function createDataTable(tableId){
    return $('#'+tableId).DataTable({
        responsive: true,
        lengthMenu: [5,10, 25, -1],
        scrollCollapse: true,
        scrollY: '415px',
        "columnDefs": [
          { "orderable": false, "targets": -1 }
        ],
        layout: {
            bottomStart:null,
            topStart: null,
            bottomEnd: [
                {
                    info:{
                        text : "Showing _START_ - _END_ of _TOTAL_"
                    }
                    
                },
                {
                    pageLength:{
                        text: "_MENU_",
                    }
                },
                {
                    paging:{
                        buttons: 3
                    }
                }
            ],
        }
      });
}
