(function (app) {

  var usersURL = "/api/v1/users";

  app.login = function () {

    var data = {
      email: $("#email").val(),
      password: $("#password").val(),
      twoFactorToken: $("#authentication-code").val()
    };

    callAPI(`${usersURL}/login`, "POST", data, function (authToken) {

      localStorage.setItem("auth-token", authToken);
      window.location.href = "/";

    }, function (error) {

      if (error.status === 427) {
        $("#two-factor-authentication-block").show();
      }

      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["Email", "Password", "TwoFactorToken", "errors"].forEach(function (s) {
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
      beforeSend: function (request) {
        request.setRequestHeader("Authorization", `Bearer ${localStorage.getItem("auth-token")}`);
      },
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: successCallback,
      error: errorCallback
    });
  }

})(app = window.app || {});