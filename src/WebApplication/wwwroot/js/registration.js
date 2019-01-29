(function (app) {

  var usersURL = "/api/v1/users";

  app.createUser = function () {

    var data = {
      name: $("#name").val(),
      email: $("#email").val(),
      password: $("#password").val(),
      confirmPassword: $("#confirm-password").val()
    };

    callAPI(usersURL, "POST", data, function (authToken) {

      localStorage.setItem("auth-token", authToken);
      window.location.href = "/";

    }, function (error) {
      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["Name", "Email", "Password", "ConfirmPassword", "errors"].forEach(function (s) {
        if (responseJson[s] !== undefined) {
          showErrors(responseJson[s]);
        }
      });
    });
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