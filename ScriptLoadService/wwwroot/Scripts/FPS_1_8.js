// `sCode`
(function () {
    var xdcGTUID = '`xdcGTUID`';
    var xdcRegSrvc = '`xdcRegSrvc`';
    var xdcDomain = '`xdcDomain`';
    var xdcPath = '`xdcPath`';
    var tagBrand = '`brand`';
    var tagCountry = '`country`';
    var cookieDaysToLive = '`cookieDaysToLive`';
    var personalizationActive = '`personalizationActive`';
    var setExRefHasUpdatedCookie = false;
    var ccpaDomains = [`CCPA_DOMAINS`];

    var hasRDNS = ccpaDomains.filter(function (entry) {
        try {
            return entry._site === window.location.hostname;
        } catch (err) {
            return false;
        }
    }).length !== 0;

    var now = new Date();
    var cookie = {
        domain: (function () {
            var i = 0, domain = document.domain, p = domain.split('.'), s = '_gd' + now.getTime();
            while (i < (p.length - 1) && document.cookie.indexOf(s + '=' + s) == -1) {
                domain = p.slice(-1 - (++i)).join('.');
                document.cookie = s + "=" + s + ";domain=" + domain + ";";
            }
            document.cookie = s + "=;expires=Thu, 01 Jan 1970 00:00:01 GMT;domain=" + domain + ";";
            return domain;
        })(),
        set: function (sKey, sValue, vEnd, sPath, sDomain, bSecure) {
            if (!sKey || /^(?:expires|max\-age|path|domain|secure)$/i.test(sKey)) { return false; }
            var sExpires = "";
            var sSameSite = "None";
            if (vEnd) {
                switch (vEnd.constructor) {
                    case Number:
                        sExpires = vEnd === Infinity ? "; expires=Fri, 31 Dec 9999 23:59:59 GMT" : "; max-age=" + vEnd;
                        break;
                    case String:
                        sExpires = "; expires=" + vEnd;
                        break;
                    case Date:
                        sExpires = "; expires=" + vEnd.toUTCString();
                        break;
                }
            }
            if (sSameSite === "None")
                bSecure = true;
            document.cookie = encodeURIComponent(sKey) + "=" + encodeURIComponent(sValue) + sExpires + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "") + (bSecure ? "; secure" : "") + (sSameSite ? "; samesite=" + sSameSite : "None");
            return true;
        },
        get: function (name) {
            var nameEQ = name + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
            }
            return null;
        }
    };

    var UUIDV4 = function b(a) { return a ? (a ^ Math.random() * 16 >> a / 4).toString(16) : ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, b) };

    var gt_dns = cookie.get("gt_dns");
    var currentGUID = cookie.get('gt_uid');

    if (hasRDNS === true && gt_dns !== true) {

        cookie.set('gt_dns', true, new Date(now.getTime() + (cookieDaysToLive * 86400000)), '/', cookie.domain);

        gt_dns = "true";

        if (currentGUID === xdcGTUID) {
            var newId = UUIDV4();
            cookie.set('gt_uid', newId, new Date(now.getTime() + (cookieDaysToLive * 86400000)), '/', cookie.domain);
        }
    }

    // prevent cross domain request when local dns cookie is set
    //
    var skipXDCRequest = typeof gt_dns === "string" && gt_dns.toLocaleLowerCase() === "true";

    var registerXDC = function (xdcGuid, preventRequest) {

        if (preventRequest === true) return false;

        $.ajax({
            url: xdcRegSrvc + "?regFPSID=" + xdcGuid,
            crossDomain: true,
            xhrFields: { withCredentials: true },
            dataType: 'JSONP',
            jsonpCallback: 'callback',
            success: function (data) { }
        });
    };

    var uid = cookie.get('gt_uid');

    if (xdcDomain.length > 0 && skipXDCRequest === false) {
        if (!uid) {
            if (xdcGTUID.length > 0) {
                uid = xdcGTUID;
            } else {
                uid = UUIDV4();
                registerXDC(uid);
            }
        } else {
            if (xdcGTUID.length > 0) {
                uid = xdcGTUID;
            } else {
                registerXDC(uid);
            }
        }
    }
    else if (!uid) {
        uid = UUIDV4();
    }

    if (personalizationActive && uid) {
        cookie.set('gt_uid', uid, new Date(now.getTime() + (cookieDaysToLive * 86400000)), "/", cookie.domain);
    }

    var ajaxConfig = { type: 'POST', dataType: 'json', contentType: 'application/json; charset=UTF-8' };
    var ajaxData = { uid: uid, tagBrand: tagBrand, tagCountry: tagCountry, referrer: document.referrer };
    var djb2 = function (str) {
        var hash = 5381;
        for (i = 0; i < str.length; i++) {
            c = str.charCodeAt(i);
            hash = ((hash << 5) + hash) + c;
        }
        return hash;
    };
    var stringifyEach = function (keyValueObjects) {
        if (jQuery.isArray(keyValueObjects)) {
            jQuery.each(keyValueObjects, function (index, keyValueObject) {
                for (var key in keyValueObject) {
                    if (jQuery.type(keyValueObject[key]) !== "string") {
                        keyValueObject[key] = JSON.stringify(keyValueObject[key]);
                    }
                }
            });
        }
        return keyValueObjects;
    };
    var core = {
        config: {
            path: '',
            testMode: false
        },
        lib: {
            ExternalRef: function (type, id) {
                return { 'ExternalRef': { _type: type, id: djb2(id) } };
            },
            ExternalRefUnhashed: function (type, id) {
                return { 'ExternalRef': { _type: type, id: id, djb2: djb2(id) } };
            }
        },
        reservedProperties: ['metadata', 'reservedKey', 'trueKey', 'params', 'match', 'suffix', 'query', 'suffixFilter'],
        set: function (entries, _timeout) {
            var config = jQuery.extend(ajaxConfig, {
                url: core.config.path + '/api/personalization_1_8/set',
                timeout: (jQuery.type(_timeout) === "number") ? _timeout : 5000
            });
            config.data = JSON.stringify(jQuery.extend(ajaxData, {
                entries: entries,
                url: document.location.href,
                testMode: core.config.testMode
            }));
            var defer = jQuery.ajax(config);
            return defer;
        },
        get: function (keyValueObjects, _timeout, asBrand, asCountry) {
            var entries = stringifyEach(keyValueObjects);
            var config = jQuery.extend(ajaxConfig, {
                url: core.config.path + '/api/personalization_1_8/get?uid=' + uid,
                timeout: (jQuery.type(_timeout) === "number") ? _timeout : 3000
            });
            config.data = JSON.stringify(jQuery.extend(ajaxData, {
                entries: entries,
                tagBrand: asBrand || tagBrand,
                tagCountry: asCountry || tagCountry,
                testMode: core.config.testMode
            }));
            var defer = jQuery.ajax(config);
            return defer;
        },
        setExternalRef: function (entries, _timeout) {
            var config = jQuery.extend(ajaxConfig, {
                url: core.config.path + '/api/externalref/set',
                timeout: (jQuery.type(_timeout) === "number") ? _timeout : 5000,
                testMode: core.config.testMode
            });
            config.data = JSON.stringify(jQuery.extend(ajaxData, {
                entries: entries
            }));
            var defer = jQuery.ajax(config);
            return defer;
        }
    };
    var getFPSKey = function (fpsObject) {
        for (var key in fpsObject) {
            if (jQuery.inArray(key, core.reservedProperties) < 0) {
                return key;
            }
        }
        return null;
    };
    var storageKeyPrefix = 'FPS_Cache__';
    var storageKey = function (reservedKey) {
        return storageKeyPrefix + reservedKey;
    };
    window.FPS = {
        config: {
            path: xdcPath.length > 0 ? xdcPath : '/fps'
        },
        actions: {
            BPComplete: { name: 'BPComplete', score: '3' },
            BPStart: { name: 'BPStart', score: '2' },
            Referral: { name: 'Referral', score: '2' },
            RequestBrochure: { name: 'RequestBrochure', score: '3' },
            RequestPaymentEstimate: { name: 'RequestPaymentEstimate', score: '3' },
            RequestQuickQuote: { name: 'RequestQuickQuote', score: '5' },
            RequestQuote: { name: 'RequestQuote', score: '4' },
            RequestUpdates: { name: 'RequestUpdates', score: '5' },
            ScheduleTestDrive: { name: 'ScheduleTestDrive', score: '4' },
            SearchDealer: { name: 'SearchDealer', score: '2' },
            SearchInventory: { name: 'SearchInventory', score: '3' },
            SearchSite: { name: 'SearchSite', score: '2' },
            ViewPage: { name: 'ViewPage', score: '1' }
        },
        lib: {
            ActionNameplate: function (year, brand, nameplate, action, returnURL) {
                return { 'ActionNameplate': { _year: year, _brand: brand, _nameplate: nameplate, returnURL: returnURL }, suffix: action.name, metadata: { score: action.score } };
            },
            ActionTrim: function (year, brand, nameplate, trim, action, returnURL) {
                return { 'ActionTrim': { _year: year, _brand: brand, _nameplate: nameplate, _trim: trim, returnURL: returnURL }, suffix: action.name, metadata: { score: action.score } };
            },
            PageVisit: function (omniturePageName) {
                return { 'PageVisit': { _omniturePageName: omniturePageName } };
            },
            PreferredDealer: function (paCode, description) {
                return { 'PreferredDealer': { _paCode: paCode, description: description } };
            },
            UserDefinedLocation: function (name, description) {
                return { 'UserDefinedLocation': { _name: name, description: description } };
            },
            ViewedVehicle: function (year, brand, nameplate, trim) {
                return { 'ViewedVehicle': { _year: year, _brand: brand, _nameplate: nameplate, _trim: trim } };
            },
            Visited: function (url) {
                return { 'Visited': { _url: url } };
            }
        },
        reserved: {
            LastViewedVehicle: { reservedKey: 'LastViewedVehicle', trueKey: 'ViewedVehicle', params: { max: 1 } },
            RecentlyViewedVehicles: { reservedKey: 'RecentlyViewedVehicles', trueKey: 'ViewedVehicle', params: { max: 4 } },
            VOIAggregateNameplate: { reservedKey: 'VOIAggregateNameplate', trueKey: 'ActionNameplate', params: { query: 'ScoreAggregate:p*', max: 1 } },
            VOIAggregateTrim: { reservedKey: 'VOIAggregateTrim', trueKey: 'ActionTrim', params: { query: 'ScoreAggregate:p*', max: 1 } },
            VOISimpleNameplate: { reservedKey: 'VOISimpleNameplate', trueKey: 'ActionNameplate', params: { query: 'Score', max: 1 } },
            VOISimpleTrim: { reservedKey: 'VOISimpleTrim', trueKey: 'ActionTrim', params: { query: 'Score', max: 1 } }
        },
        get: function (keyValueObjects, callbacks, asBrand, asCountry) {
            if (callbacks && callbacks.success && callbacks.error) {
                var entries = [];
                var newEntry;
                var inStorage = [];
                var getAs = (asBrand !== undefined) && (asCountry !== undefined);
                var reservedObject = undefined;
                var indices = {};
                var newIndex = 0;
                var extract = function (data) {
                    var extracted = [];
                    if (data) {
                        jQuery.each(data, function (index, kvPair) {
                            var formattedObject = new Object();
                            formattedObject[kvPair['Key']] = kvPair['Value'];
                            extracted.push(formattedObject);
                        });
                    }
                    return extracted;
                };
                var complete = function (success, callbacks, data) {
                    var jqDef = new $.Deferred();
                    var jqXHR = jqDef.promise();

                    jqXHR.success = jqXHR.done;
                    jqXHR.error = jqXHR.fail;

                    var s = $.ajaxSetup({}, {});
                    var callbackContext = s.context || s;

                    jqXHR.readyState = success ? 4 : 0;
                    jqXHR.status = success ? 200 : 0;
                    jqXHR.statusText = success ? 'OK' : 'error';
                    jqXHR.responseJSON = data;
                    jqXHR.responseText = JSON.stringify(data);

                    jqXHR.success(function (data, statusText, jqXHR) {
                        callbacks.success(extract(data), statusText, jqXHR);
                    });

                    jqDef.resolveWith(callbackContext, [data, success ? 'success' : 'error', jqXHR]);

                    return jqXHR;
                };

                if (!Array.isArray(keyValueObjects)) {
                    keyValueObjects = [keyValueObjects];
                }
                jQuery.each(keyValueObjects, function (index, keyValueObject) {
                    var fpsKey;
                    var storageValue;
                    var storageValueTimestamp;

                    if (keyValueObject.hasOwnProperty('match')) {
                        fpsKey = getFPSKey(keyValueObject['match']);
                        newEntry = {
                            key: 'match',
                            value: keyValueObject['match'],
                            suffix: keyValueObject['match']['suffix'],
                            query: keyValueObject.hasOwnProperty('query') ? keyValueObject['query'] : ''
                        };
                    }
                    else if (keyValueObject.hasOwnProperty('reservedKey')) {
                        fpsKey = keyValueObject['reservedKey'];
                        newEntry = {
                            key: keyValueObject['trueKey'],
                            value: keyValueObject['params'],
                            reservedKey: fpsKey
                        };
                    }
                    else {
                        fpsKey = getFPSKey(keyValueObject);
                        reservedObject = FPS.reserved[fpsKey];
                        if (reservedObject) {
                            newEntry = {
                                key: reservedObject['trueKey'],
                                value: reservedObject['params'],
                                reservedKey: fpsKey
                            };
                        }
                        else {
                            newEntry = {
                                key: fpsKey,
                                value: keyValueObject[fpsKey],
                                suffix: keyValueObject['suffix']
                            };
                        }
                    }

                    if (!indices.hasOwnProperty(fpsKey)) {
                        indices[fpsKey] = newIndex;
                        ++newIndex;
                    }
                    if (newEntry['reservedKey'] && !getAs && localStorage) {
                        storageValue = localStorage.getItem(storageKey(newEntry['reservedKey']));
                        storageValueTimestamp = parseInt(localStorage.getItem(storageKey(newEntry['reservedKey']) + '_TS') || 0);
                    }
                    if (storageValue && (Date.now() - storageValueTimestamp) <= 0) {
                        inStorage.push({
                            Key: newEntry['reservedKey'],
                            Value: JSON.parse(storageValue)
                        });
                    }
                    else {
                        entries.push(newEntry);
                    }
                });

                if (entries.length === 0 && inStorage.length > 0) {
                    complete(true, callbacks, inStorage);
                }
                else {
                    jQuery.extend(core.config, FPS.config);
                    core.get(entries, callbacks.timeout, asBrand, asCountry).done(function (data, statusText, jqXHR) {
                        var combined = [];
                        jQuery.each(data, function (index, kvPair) {
                            if (!getAs) {
                                if (localStorage && FPS.reserved.hasOwnProperty(kvPair.Key)) {
                                    localStorage.setItem(storageKey(kvPair.Key), JSON.stringify(kvPair.Value));
                                    localStorage.setItem(storageKey(kvPair.Key) + '_TS', Date.now());
                                }
                                if (FPS.hasOwnProperty('targetCookies') && FPS.targetCookies.hasOwnProperty(kvPair.Key)) {
                                    FPS.targetCookies[kvPair.Key](kvPair);
                                }
                            }
                            combined[indices[kvPair.Key]] = kvPair;
                        });
                        jQuery.each(inStorage, function (index, kvPair) {
                            combined[indices[kvPair.Key]] = kvPair;
                        });
                        callbacks.success(extract(combined), statusText, jqXHR);
                    }).fail(function (jqXHR, statusText, error) {
                        callbacks.error(jqXHR, statusText, error);
                    });
                }
            }
        },
        set: function (keyValueObjects, callbacks) {
            var entries = [];
            if (!Array.isArray(keyValueObjects)) {
                keyValueObjects = [keyValueObjects];
            }
            jQuery.each(stringifyEach(keyValueObjects), function (index, keyValueObject) {
                var reservedKey;
                var fpsKey = getFPSKey(keyValueObject);
                if (fpsKey) {
                    entries.push({
                        key: fpsKey,
                        value: keyValueObject[fpsKey],
                        metadata: keyValueObject['metadata'],
                        suffix: keyValueObject['suffix']
                    });

                    if (localStorage) {
                        for (reservedKey in FPS.reserved) {
                            if (FPS.reserved[reservedKey]['trueKey'] === fpsKey) {
                                localStorage.removeItem(storageKey(reservedKey));
                            }
                        }
                    }
                }
            });

            jQuery.extend(core.config, FPS.config);
            if (callbacks && callbacks.success && callbacks.error) {
                core.set(entries, callbacks.timeout).done(function (data, statusText, jqXHR) {
                    callbacks.success(data, statusText, jqXHR);
                }).fail(function (jqXHR, statusText, error) {
                    callbacks.error(jqXHR, statusText, error);
                });
            }
            else {
                core.set(entries).done(function (data, statusText, jqXHR) {
                });
            }
        },
        getAll: function (callbacks) {
            FPS.get([
                { 'Visited': { max: 1 } },
                { 'UserDefinedLocation': { max: 1 } },
                FPS.reserved.RecentlyViewedVehicles,
                { 'PreferredDealer': { max: 10 } },
                FPS.reserved.LastViewedVehicle
            ], callbacks);
        },
        setExternalRef: function (type, id, callbacks, unhashed) {
            if (type && id && callbacks && callbacks.success && callbacks.error) {
                jQuery.extend(core.config, FPS.config);
                unhashed = typeof unhashed !== 'undefined' ? unhashed : true;
                core.setExternalRef([{ key: 'ExternalRef', value: JSON.stringify(core.lib['ExternalRef' + (unhashed === true ? 'Unhashed' : '')](type, id)['ExternalRef']) }], callbacks.timeout).done(function (data, statusText, jqXHR) {
                    if (data && data.uid_submitted && data.uid_reconciled) {

                        var gt_dns = cookie.get("gt_dns");
                        var dnsModeIsActive = gt_dns != null;
                        var xdcModeIsActive = xdcDomain.length > 0;
                        var tier3DNSModeIsActive = xdcModeIsActive && dnsModeIsActive;

                        setExRefHasUpdatedCookie = true;

                        if (dnsModeIsActive && (gt_dns === data.uid_reconciled || gt_dns === data.uid_submitted)) {
                            cookie.set('gt_dns', xdcModeIsActive ? true : UUIDV4(), new Date(now.getTime() + (cookieDaysToLive * 86400000)), '/', cookie.domain);
                        }

                        if (data.uid_submitted !== data.uid_reconciled) {

                            uid = tier3DNSModeIsActive ? data.uid_submitted : data.uid_reconciled;

                            cookie.set('gt_uid', uid, new Date(now.getTime() + (cookieDaysToLive * 86400000)), '/', cookie.domain);
                            ajaxData.uid = uid;

                            if (localStorage) {
                                for (sessionKey in localStorage) {
                                    if (sessionKey.slice(0, storageKeyPrefix.length) === storageKeyPrefix) {
                                        localStorage.removeItem(sessionKey);
                                    }
                                }
                            }
                        } else if (data.uid_submitted === data.uid_reconciled && tier3DNSModeIsActive) {

                            uid = UUIDV4();

                            cookie.set('gt_uid', uid, new Date(now.getTime() + (cookieDaysToLive * 86400000)), '/', cookie.domain);
                            ajaxData.uid = uid;

                            if (localStorage) {
                                for (sessionKey in localStorage) {
                                    if (sessionKey.slice(0, storageKeyPrefix.length) === storageKeyPrefix) {
                                        localStorage.removeItem(sessionKey);
                                    }
                                }
                            }
                        }

                        if (FPS.hasOwnProperty('setTargetCookie')) {
                            FPS.setTargetCookie({ success: function () { }, error: function () { } });
                        }
                    }
                    callbacks.success(data, statusText, jqXHR);
                }).fail(function (jqXHR, statusText, error) {
                    callbacks.error(jqXHR, statusText, error);
                });
            }
        }
    };
    // BASE
    // PACKAGES
    // EXTENSIONS
    if (!personalizationActive) {
        // ACTIVE?
    }
    // SETTING_ONLOAD
    // SITE_ONLOAD
}());