var viewer;

document.addEventListener("DOMContentLoaded", function (event) {

    function getParameterByName(name) {
        url = window.location.href;
        name = name.replace(/[\[\]]/g, '\\$&');
        var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    function launchViewer(urn) {
        const options = {
            env: 'AutodeskProduction',
            getAccessToken: getForgeToken
        };
        Autodesk.Viewing.Initializer(options, function () {
            viewer = new Autodesk.Viewing.GuiViewer3D(document.getElementById('forgeViewer'));
            viewer.loadExtension('Autodesk.DocumentBrowser');
            viewer.start();
            loadDocument('urn:' + urn);
        });
    }

    function loadDocument(documentId) {
        Autodesk.Viewing.Document.load(
            documentId,
            function onSuccess(doc) {
                const defaultGeom = doc.getRoot().getDefaultGeometry();
                viewer.loadDocumentNode(doc, defaultGeom);
            },
            function onError(err) {
                console.error(err);
            }
        );
    }

    async function getForgeToken(callback) {
        const resp = await fetch('api/oauth/viewer');
        if (!resp.ok) {
            const msg = await resp.text();
            console.error('Could not obtain access token', msg);
            return;
        }
        const credentials = await resp.json();
        callback(credentials.access_token, credentials.expires_in);
    }

    var url = getParameterByName('urn');
    launchViewer(url);


});