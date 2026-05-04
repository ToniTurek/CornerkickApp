function getIntFromString(s) {
  if (!s) return 0;

  s = s.toString();

  s = s.replace(/\€/g, ''); // Remove currency symbol
  s = s.replace(/\%/g, ''); // Remove percent sign
  s = s.replace(/\px/g, ''); // Remove px
  /*
  try {
    var f = parseFloat(s);
    s = f.toFixed(0);
  } catch {
  }
  */
  s = s.replace(/\./g, ''); // Remove dot
  s = s.replace(/\,/g, ''); // Remove comma

  return parseInt(s);
}

function addThousandSepToNumber(s) {
  return getIntFromString(s).toLocaleString('de-DE');
}

function addThousandSepToNumberInt(i) {
  return i.toLocaleString('de-DE');
}
