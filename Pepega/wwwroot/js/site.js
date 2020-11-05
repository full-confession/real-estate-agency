// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function GoToPage(pageIndex) {

	$("#filterPageIndex").val(pageIndex);
	Submit();
}

function ApplyFilters() {
	$("#filterPageIndex").val(1);
	Submit();
}

function Submit() {
	var form = $("#filterForm");
	DisableEmptyInput(form);
	form.submit();
}

function Clear() {
	$("#filterForm").find("input.from-reset, select.from-reset").each(function (index) {
		var input = $(this);
		input.val("");
	});
}

function DisableEmptyInput(form) {
	form.find("input, select").each(function(index) {
		var input = $(this);
		input.prop("disabled", input.val() === "");
	});
}

//class PropertyAttribute {
//	constructor(id, name, type) {
//		this.id = id;
//		this.name = name;
//		this.type = type;
//	}
//}

//var BaseURL = window.location.origin;
//var PropertyAttributes = new Promise(function (resolve, reject) {

//	$.getJSON(BaseURL + "/Property/PropertyAttributes").done(function (data) {
//		var attributes = new Array();
//		console.log(data);
//		$.each(data,
//			function (i, attribute) {
//				attributes.push(new PropertyAttribute(attribute.propertyAttributeId, attribute.name, attribute.type));
//			});

//		resolve(attributes);
//	}).fail(function() {
//		reject();
//	});
//});


//class IntFilter {
//	constructor(attribute) {

//		this.attribute = attribute;
//		var listItem = $("<li></li>", { "class": "list-group-item" });
//		$("<label></label>", { text: attribute.name }).appendTo(listItem);
//		this.inputField = $("<input>", { type: "number", "class": "form-control" }).appendTo(listItem);

//		listItem.appendTo("#filter-list");
//	}

//	ToQueryString() {
//		return `attributeId=${this.attribute.id}&intValue=${this.inputField.val()}`;
//	}
//}

//var Filters = new Array();

//function AddNewFilter(attribute) {
//	Filters.push(new IntFilter(attribute));
//}

//PropertyAttributes.then(function (attributes) {
//	$.each(attributes,
//		function(i, attribute) {
//			$("#filter-list-select").append(`<option value="${i}">${attribute.name}</option>`);
//		});

//	$("#filter-list-select").change(function () {
//		AddNewFilter(attributes[this.value]);

//		$("#filter-list-select option").prop("selected", function () {
//			return this.defaultSelected;
//		});
//	});

$("#citySelect").change(function() {

	var districtSelect = $("#districtSelect");
	var streetSelect = $("#streetSelect");
	var houseInput = $("#houseInput");

	districtSelect.empty().append($("<option></option>")
		.text("Район").attr("value", ""));

	streetSelect.empty().append($("<option></option>")
		.text("Улица").attr("value", ""));

	houseInput.val("");

	streetSelect.prop("disabled", true);
	houseInput.prop("disabled", true);

	if ($(this).val() !== "") {
		$.getJSON(window.location.origin + "/Property/Districts?CityId=" + $(this).val()).done(function(districts) {

			$.each(districts,
				function(i, district) {
					districtSelect.append($("<option></option>")
						.attr("value", district.districtId)
						.text(district.name));
				});

			districtSelect.prop("disabled", false);
		});
	} else {
		districtSelect.prop("disabled", true);
	}
});

$("#districtSelect").change(function () {


	var select = $("#streetSelect");
	var houseInput = $("#houseInput");

	select.empty().append($("<option></option>")
		.text("Улица").attr("value", ""));
	houseInput.val("").prop("disabled", true);

	if ($(this).val() !== "") {
		$.getJSON(window.location.origin + "/Property/Streets?DistrictId=" + $(this).val()).done(function(streets) {

			$.each(streets,
				function(i, street) {
					select.append($("<option></option>")
						.attr("value", street.streetId)
						.text(street.name));
				});

			select.prop("disabled", false);
		});
	} else {
		select.prop("disabled", true);
	}
});

$("#streetSelect").change(function() {

	var houseInput = $("#houseInput");

	if ($(this).val() !== "") {
		houseInput.val("").prop("disabled", false);
	} else {
		houseInput.val("").prop("disabled", true);
	}

});

//	$("#filter-button").click(function () {

//		var urlString = BaseURL + "/Property/Index";
//		$.each(Filters,
//			function (i, filter) {
//				if (i === 0) {
//					urlString += "?";
//				}

//				urlString += filter.ToQueryString();

//				if (i !== Filters.length - 1) {
//					urlString += "&";
//				}
//			});

//		window.location.href = urlString;
//	});
//});

