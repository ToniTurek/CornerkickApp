export function showTrainingHistory(_ltHistData) {
  const ltHistData = JSON.parse(_ltHistData);

  var div0 = document.getElementById("trHistContainer");

  if (div0) {
    div0.innerHTML = '';
    addTrainingHistoryTable(
      div0,
      ltHistData
    );
  }
}
