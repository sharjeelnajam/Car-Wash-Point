
window.downloadHelper = (fileName, contentType, base64Data) => {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64Data}`;
    link.download = fileName;
    link.click();
};

// Opens a PDF (base64) in a new browser tab using a Blob URL.
window.viewPdfHelper = (base64Data) => {
    const byteChars = atob(base64Data);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: "application/pdf" });
    const blobUrl = URL.createObjectURL(blob);

    window.open(blobUrl, "_blank", "noopener,noreferrer");
};

// Prints a PDF (base64) through a hidden iframe so no popup window is needed
// (browsers block window.open after a Blazor Server round trip).
window.printPdfHelper = (base64Data) => {
    const byteChars = atob(base64Data);
    const byteArray = new Uint8Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteArray[i] = byteChars.charCodeAt(i);
    }

    const blobUrl = URL.createObjectURL(new Blob([byteArray], { type: "application/pdf" }));

    const frame = document.createElement("iframe");
    frame.style.cssText = "position:fixed;right:0;bottom:0;width:0;height:0;border:0;";
    frame.src = blobUrl;

    let printed = false;
    const doPrint = () => {
        if (printed) return;
        printed = true;
        try {
            frame.contentWindow.focus();
            frame.contentWindow.print();
        } catch (e) {
            // Fall back to the PDF viewer tab so the user can print from there.
            window.open(blobUrl, "_blank");
        }
        setTimeout(() => { frame.remove(); URL.revokeObjectURL(blobUrl); }, 60000);
    };

    frame.onload = () => setTimeout(doPrint, 500);
    document.body.appendChild(frame);
};
