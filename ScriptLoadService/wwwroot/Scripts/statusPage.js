$(function () {
    var statusTestPage = {
        vars: {
            numPass: 0,
            numTests: 0,
            numFailures: 0,
            testsComplete: 0,
            checkFPSReadyCounter: 0
        }
    };
    alert(window.location.search);
        //jQuery.getScript(GLOBALS.api_url + "/script/Test/TST?ns=SING")
        //jQuery.getScript(GLOBALS.api_url + "/script/Ford/USA")
    jQuery.getScript("/fps/script/Ford/USA" + window.location.search)
        .done(function (script, status) {
            checkFPSReady();
        })
        .fail(function () {
            jQuery.getScript("/script/Ford/USA" + window.location.search)
            .done(function (script, status) {
                    checkFPSReady();
            })
            .fail(function() {
                console.log("Could not download FPS");
                $("#status").html("FAIL");
                $("#failures").html("1");
            });
        });

    function checkFPSReady() {
        if (typeof window.FPS !== 'undefined') {
            startTests();
        } else {
            if (statusTestPage.vars.checkFPSReadyCounter < 20) {
                statusTestPage.vars.checkFPSReadyCounter += 1;
                setTimeout(checkFPSReady, 250);
            }
        }
    }

    function startTests() {
        console.log("Starting tests...");
        //Test for FPS being loaded
        var hasFPSLoaded = typeof window.FPS === 'undefined' ? false : true;
        updateStatus("#downloadJSSuccess", hasFPSLoaded ? "pass" : "fail");

        //Set Testmode in FPS
        FPS.config.testMode = true;   

        if (hasFPSLoaded) {
            //FPS.set([FPS.lib.ActionNameplate('2017', 'Ford', 'Mustang', FPS.actions.BPStart, 'www.ford.com')]);
            // Test 1 - Set/Get
            FPS.set([{ 'StatusTest1': { value: true } }],
            {
                success: function () {
                    updateStatus("#setTest1Result", "pass");
                    FPS.get([{ 'StatusTest1': {} }],
                        {
                            success: function (data) {
                                updateStatus("#getTest1Result", "pass");
                                $("#getTest1Data").html(JSON.stringify(data));
                            },
                            error: function () {
                                updateStatus("#getTest1Result", "fail");
                            }
                        });

                }, error: function () {
                    updateStatus("#setTest1Result", "fail");
                }
            });
        }
    }

    function updateStatus(selector, status) {
        if (typeof status === "undefined" || (status === null || status === ""
            || status === "fail" || status === "FAIL") && isNaN(status)) {
            status = "FAIL";
            statusTestPage.vars.numTests += 1;
            statusTestPage.vars.numFailures += 1;
        } else if (!isNaN(status)) {
            status = status;
        } else {
            status = "PASS";
            statusTestPage.vars.numTests += 1;
            statusTestPage.vars.numPass += 1;
        }
        $(selector).html(status);
        //overall status
        var overAllStatusPass = (statusTestPage.vars.numTests == statusTestPage.vars.numPass) ? "PASS" : "fail";
        $("#status").html(overAllStatusPass);
        //number of failures
        $("#failures").html(statusTestPage.vars.numFailures);
    }
});