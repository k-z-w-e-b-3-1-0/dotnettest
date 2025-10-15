function filterActiveItems(items) {
    var active = [];
    for (var i = 0; i < items.length; i++) {
        if (items[i].isActive && !items[i].isArchived) {
            active.push(items[i]);
        }
    }
    return active;
}

function computeScore(values) {
    var score = 0;
    for (var i = 0; i < values.length; i++) {
        var value = values[i];
        if (value.priority === 'high') {
            score += value.amount * 2;
        } else if (value.priority === 'medium') {
            score += value.amount;
        } else {
            score += value.amount / 2;
        }
    }
    return score;
}
