(function (app) {

  var loginURL = "/api/v1/users/login";

  app.login = function () {

    var data = {
      email: $("#email").val(),
      password: $("#password").val()
    };

    callAPI(loginURL, "POST", data, function (authToken) {

      localStorage.setItem("auth-token", authToken);
      window.location.href = "/";

    }, function (error) {

      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["Email", "Password", "errors"].forEach(function (s) {
        if (responseJson[s] !== undefined) {
          showErrors(responseJson[s]);
        }
      });
    });
  };

  app.logout = function () {
    localStorage.removeItem("auth-token");
    window.location.href = "/";
  };

  function showErrors(errors) {

    var $errors = $("#errors");

    errors.forEach(function (error) {
      $errors.append(
        $("<li>").append(error)
      );
    });
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