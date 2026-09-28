$(function () {
    var config = {
        authority: "http://localhost:60000/",
        //authority: "https://nightlyidsrv.ubeac.io/",
        //authority: "https://idsrv.ubeac.io/",
        client_id: "uBeacIdsrvClient",
        response_type: "password",
        scope: "openid profile roles idsrv",
        token_url: "connect/token",
        client_secret: "fah3nS)GERJnf345baf@rf$s345!ngS(DFgshj#fg458G"
    };


    function setStorage(key, value) {
        if (value) {
            localStorage.setItem(key, JSON.stringify(value));
        }
        else {
            if (localStorage.getItem(key))
                localStorage.removeItem(key);
        }
    }

    function getStorage(key) {
        var result = localStorage.getItem(key);
        if (result) {
            result = JSON.parse(result);
        }
        return result;
    }

    function login() {
        var email = document.getElementById("email").value;
        var password = document.getElementById("password").value;
        getToken(email, password);
        refresh_ui();
    }

    function logout() {
        setStorage("user_token", null);
        refresh_ui();
    }

    function getToken(email, password) {
        var xhr = new XMLHttpRequest();
        xhr.onload = function (e) {
            console.log(xhr.response);
            var response_data = JSON.parse(xhr.response);
            if (xhr.status === 200 && response_data.access_token) {
                setStorage("user_token", response_data);
                refresh_ui();
            }
        };
        xhr.open("POST", config.authority + config.token_url);
        var data = {
            username: email,
            password: password,
            grant_type: config.response_type,
            scope: config.scope
        };
        var body = "";
        for (var key in data) {
            if (body.length) {
                body += "&";
            }
            body += key + "=";
            body += encodeURIComponent(data[key]);
        }
        xhr.setRequestHeader("Authorization", "Basic " + btoa(config.client_id + ":" + config.client_secret));
        xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        xhr.send(body);

    }

    function refresh_ui() {
        if (getStorage("user_token")) {
            $("#login-form").hide();
            $("#logout-form").show();
        }
        else {
            $("#login-form").show();
            $("#logout-form").hide();
        }
    }

    const constantMock = window.fetch;
    window.fetch = function () {
        var user_token = getStorage("user_token");
        if (user_token && user_token.token_type && user_token.access_token) {
            var auth_key = "Authorization";
            var auth_token = user_token.token_type + " " + user_token.access_token;
            arguments[1].headers[auth_key] = auth_token;
        }

        return constantMock.apply(this, arguments);
    };

    var basicAuthUI =
        '<div id="login-form"><div class="input"><input placeholder="Email" id="email" name="username" type="text" size="10"></div>' +
        '<div class="input"><input placeholder="Password" id="password" name="password" type="password" size="10"></div>' +
        '<button class="btn execute" id="btn-login" type="button">Login</button></div>';
    var logoutUI = '<div id="logout-form"><button class="btn execute" id="btn-logout" type="button">Logout</button></div>';

    var delayInMilliseconds = 1000; //1 second

    setTimeout(function () {
        $(basicAuthUI).insertAfter('#select');
        $(logoutUI).insertAfter('#select');
        //your code to be executed after 1 second
        document.getElementById("btn-login").addEventListener("click", login, false);
        document.getElementById("btn-logout").addEventListener("click", logout, false);

        refresh_ui();
    }, delayInMilliseconds);

});


