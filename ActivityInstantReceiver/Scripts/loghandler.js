function logout() {
    Cookies.remove('login');
    document.location.href = "entrance.html";
}

function navbarHandle() {
    const menu = ["查看列表"];
    const menu_href = ["eventCenter.html"];
    const Admin_menu = ["帳號管理", "列表管理"];
    const Admin_menu_href = ["manage_account.html", "manage.html"];
    let LOGIN = JSON.parse(Cookies.get('login'));
    if (LOGIN.authority === 1) {
        navbarCreate(Admin_menu, Admin_menu_href);
    }
    navbarCreate(menu, menu_href);
}

function navbarCreate(list, list_href) {
    for (let i = 0; i < list.length; ++i) {
        $("#nav-mobile").prepend('<li><a href="' + list_href[i] + '">' + list[i] + '</a></li>');
    }
}