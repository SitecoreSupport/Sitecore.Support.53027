define(["sitecore", "jquery"], function (sc, $) {
  sc.Factories.createBaseComponent({
    name: "AccountInformation",
    base: "ControlBase",
    selector: ".sc-accountInformation",
    logout: function (target, event) {
      event.preventDefault();

      var ajaxSettings = {
        type: "POST",
        url: "/sitecore/shell/api/sitecore/SupportAuthentication/Logout?sc_database=master",
        data: {},
        cache: false
      };

      var token = sc.Helpers.antiForgery.getAntiForgeryToken();
      ajaxSettings.data[token.formKey] = token.value;

      $.ajax(ajaxSettings).done(function (data) {
          window.location = JSON.parse(data).Redirect;
          window.top.location = JSON.parse(data).Redirect;
      });
    }
  });
});
