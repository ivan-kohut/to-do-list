(function (app) {

  var usersURL = "/api/v1/users";

  app.loginByGoogle = function () {

    var data = {
      code: getParameterByName("code"),
      redirectUri: "https://localhost:44388/googlecallback"
    };

    callAPI(`${usersURL}/google-login`, "POST", data, function (authToken) {

      localStorage.setItem("auth-token", authToken);
      window.location.href = "/";

    }, function () {
      window.location.href = "/";
    });
  };

  function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&#]" + name + "(=([^&#]*)|&|#|$)"),
      results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
  }

  function callAPI(url, method, data, successCallback, errorCallback) {
    $.ajax({
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: successCallback,
      error: errorCallback
    });
  }

})(app = window.app || {});
