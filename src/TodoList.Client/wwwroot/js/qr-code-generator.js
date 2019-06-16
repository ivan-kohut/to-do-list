window.generateQrCode = function (block, authenticatorUri) {
  new QRCode(block,
    {
      text: authenticatorUri,
      width: 150,
      height: 150
    });
};
