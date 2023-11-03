function copyToClipboard(text) {
    var input = document.createElement("input");
    input.style.position = "fixed";
    input.style.opacity = 0;
    input.value = text;
    document.body.appendChild(input);

    input.select();
    document.execCommand("copy");

    document.body.removeChild(input);
}
