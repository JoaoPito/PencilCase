function downloadString(content, fileName) {
    const blob = new Blob([content]);
    const url = window.URL.createObjectURL(blob);
    const anchorElem = document.createElement('a');
    anchorElem.href = url;
    anchorElem.download = fileName ?? '';
    anchorElem.click();
    anchorElem.remove();
    window.URL.revokeObjectURL(url);
}