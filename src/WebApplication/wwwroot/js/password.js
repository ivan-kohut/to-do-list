(function (app) {

  var loginURL = "/api/v1/users";

  app.generateNewPassword = function () {

    var data = {
      email: $("#email").val()
    };

    callAPI(`${loginURL}/password-recovery`, "POST", data, function () {

      $("#email-block").hide();
      $("#login-block").show();

    }, function (error) {

      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["Email", "errors"].forEach(function (s) {
        if (responseJson[s] !== undefined) {
          showErrors(responseJson[s]);
        }
      });
    });
  };

  app.changePassword = function () {

    var data = {
      oldPassword: $("#old-password").val(),
      newPassword: $("#new-password").val(),
      confirmNewPassword: $("#confirm-new-password").val()
    };

    callAPI(`${loginURL}/change-password`, "POST", data, function () {

      window.location.href = "/";

    }, function (error) {

      var responseJson = error.responseJSON;

      $("#errors").empty();

      ["OldPassword", "NewPassword", "ConfirmNewPassword", "errors"].forEach(function (s) {
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

  function callAPI(url, method, data, callback, errorCallback) {
    $.ajax({
      beforeSend: function (request) {
        request.setRequestHeader("Authorization", `Bearer ${getAuthToken()}`);
      },
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback,
      error: errorCallback
    });
  }

  function getAuthToken() {
    return localStorage.getItem("auth-token");
  }

})(app = window.app || {});