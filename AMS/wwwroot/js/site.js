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