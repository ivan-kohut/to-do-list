(function (app) {

  var usersURL = "/api/v1/users";

  app.generateNewPassword = function () {

    var data = {
      email: $("#email").val()
    };

    callAPI(`${usersURL}/password-recovery`, "POST", data, function () {

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

  function callAPI(url, method, data, callback, errorCallback) {
    $.ajax({
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback,
      error: errorCallback
    });
  }

})(app = window.app || {});