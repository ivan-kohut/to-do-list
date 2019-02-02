(function (app) {

  var loginURL = "/api/v1/users/password-recovery";

  app.generateNewPassword = function () {

    var data = {
      email: $("#email").val()
    };

    callAPI(loginURL, "POST", data, function () {

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