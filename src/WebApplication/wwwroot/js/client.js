(function (app) {

  var serverURL = "/items";
  var selectedId = -1; // not selected

  var $inputField;

  app.init = function () {
    $inputField = $('#text_input');

    callAPI(serverURL, "GET", null, function (data) {
      for (var i = 0; i < data.length; i++) {
        putItem(data[i]);
      }
    });
  };

  app.addItem = function () {
    var itemText = $inputField.val();

    if (!itemText)
      return false;

    callAPI(serverURL, "POST", { text: itemText }, function (data) {
      putItem(data);
      clearInputField();

      if (isSelectedItem())
        resetItemState();
    });
  };

  app.updateItem = function () {
    var newTextValue = $inputField.val();

    if (!isSelectedItem() || !newTextValue)
      return false;

    var item = getSelectedItem();
    var data = { text: newTextValue, priority: item.attr("data-priority") };

    callAPI(`${serverURL}/${selectedId}`, "PUT", data, function () {
      item.text(newTextValue);
    });
  };

  app.deleteItem = function () {
    if (!isSelectedItem())
      return false;

    callAPI(`${serverURL}/${selectedId}`, "DELETE", null, function () {
      var item = getSelectedItem();
      var nextItem = item.next();

      item.remove();

      if (nextItem.length === 0) {
        resetItemState();
        clearInputField();
      } else {
        selectedId = nextItem.attr('id');
        $inputField.val(getSelectedItem().text());
      }
    });
  };

  app.moveUpItem = function () {
    if (!isSelectedItem())
      return false;

    var item = getSelectedItem();
    var previousItem = item.prev();

    if (previousItem.length !== 0) {
      var data = { text: item.text(), priority: previousItem.attr("data-priority") };

      callAPI(`${serverURL}/${selectedId}`, "PUT", data, function () {
        previousItem.attr("data-priority", item.attr("data-priority"));
        item.attr("data-priority", data.priority);

        item.insertBefore(previousItem);
      });
    }
  };

  app.moveDownItem = function () {
    if (!isSelectedItem())
      return false;

    var item = getSelectedItem();
    var nextItem = item.next();

    if (nextItem.length !== 0) {
      var data = { text: item.text(), priority: nextItem.attr("data-priority") };

      callAPI(`${serverURL}/${selectedId}`, "PUT", data, function () {
        nextItem.attr("data-priority", item.attr("data-priority"));
        item.attr("data-priority", data.priority);

        item.insertAfter(nextItem);
      });
    }
  };

  app.onItemSelect = function (id) {
    if (!isSelectedItem())
      changeColor("red", "blue");

    selectedId = id;
    $inputField.val(getSelectedItem().text());
  };

  function putItem(dataRow) {
    $("#item-list").append(
      $("<li>")
        .attr("id", dataRow.id)
        .attr("data-priority", dataRow.priority)
        .attr("onclick", "app.onItemSelect(" + dataRow.id + ")")
        .append(dataRow.text)
    );
  }

  function changeColor(from, to) {
    ['#update_button', '#delete_button', '#up_button', '#down_button'].forEach(function (selector) {
      $(selector).removeClass(from);
      $(selector).addClass(to);
    });
  }

  function isSelectedItem() {
    return selectedId !== -1;
  }

  function resetItemState() {
    selectedId = -1;
    changeColor("blue", "red");
  }

  function clearInputField() {
    $inputField.val("");
  }

  function getSelectedItem() {
    return $("#" + selectedId);
  }

  function callAPI(url, method, data, callback) {
    $.ajax({
      url: url,
      type: method,
      contentType: 'application/json',
      data: JSON.stringify(data),
      success: callback
    });
  }
})(app = window.app || {});