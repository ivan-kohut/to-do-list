(function (app) {

  var serverURL = "/api/v1/items";
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

  app.updateItemText = function () {
    var newTextValue = $inputField.val();

    if (!isSelectedItem() || !newTextValue)
      return false;

    var item = getSelectedItem();

    callAPI(`${serverURL}/${selectedId}`, "PATCH", [{ name: "Text", value: newTextValue }], function () {
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

      var selectedItemReplaceOperation = { name: "Priority", value: parseInt(previousItem.attr("data-priority")) };
      var previousItemReplaceOperation = { name: "Priority", value: parseInt(item.attr("data-priority")) };

      callAPI(`${serverURL}/${selectedId}`, "PATCH", [selectedItemReplaceOperation], function () {
        item.attr("data-priority", selectedItemReplaceOperation.value);
        item.insertBefore(previousItem);
      });

      callAPI(`${serverURL}/${previousItem.attr("id")}`, "PATCH", [previousItemReplaceOperation], function () {
        previousItem.attr("data-priority", previousItemReplaceOperation.value);
      });
    }
  };

  app.moveDownItem = function () {
    if (!isSelectedItem())
      return false;

    var item = getSelectedItem();
    var nextItem = item.next();

    if (nextItem.length !== 0) {

      var selectedItemReplaceOperation = { name: "Priority", value: parseInt(nextItem.attr("data-priority")) };
      var nextItemReplaceOperation = { name: "Priority", value: parseInt(item.attr("data-priority")) };

      callAPI(`${serverURL}/${selectedId}`, "PATCH", [selectedItemReplaceOperation], function () {
        item.attr("data-priority", selectedItemReplaceOperation.value);
        item.insertAfter(nextItem);
      });

      callAPI(`${serverURL}/${nextItem.attr("id")}`, "PATCH", [nextItemReplaceOperation], function () {
        nextItem.attr("data-priority", nextItemReplaceOperation.value);
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