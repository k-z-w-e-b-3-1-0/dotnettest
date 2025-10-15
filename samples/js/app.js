function initializeDashboard(widgets) {
    for (var i = 0; i < widgets.length; i++) {
        var widget = widgets[i];
        if (widget.enabled) {
            renderWidget(widget);
        } else if (widget.optional) {
            console.log('Skipping optional widget');
        } else {
            console.warn('Disabled widget', widget.name);
        }
    }
}

function renderWidget(widget) {
    switch (widget.type) {
        case 'chart':
            loadChart(widget);
            break;
        case 'list':
            loadList(widget);
            break;
        default:
            console.log('Unknown widget');
    }
}
