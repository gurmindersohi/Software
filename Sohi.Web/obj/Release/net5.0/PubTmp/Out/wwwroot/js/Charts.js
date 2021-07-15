


function DrawChart(datatodisplay, labelstodisplay, headerLabel) {

    document.getElementById("myChart").remove();
    let canvas = document.createElement('canvas');
    canvas.setAttribute('id', 'myChart');
    document.querySelector('#chart-container').appendChild(canvas);

    var ctx = document.getElementById('myChart').getContext('2d');


    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labelstodisplay,
            datasets: [{
                label: headerLabel,
                data: datatodisplay,
                backgroundColor: 'rgb(255, 99, 132)',
                borderColor: 'rgb(255, 99, 132)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMin: 5,
                    ticks: {
                        callback: function (label, index, labels) {
                            if (Math.floor(label) === label) {
                                return label;
                            }
                    }
                }
                    
                }
            }
        }
    });


}


//function DrawChart(datatodisplay, labelstodisplay) {
//    var ctx = document.getElementById('myChart').getContext('2d');

//    var myChart = new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: labelstodisplay,
//            datasets: [{
//                label: 'Number of Views',
//                data: datatodisplay,
//                backgroundColor: 'rgb(255, 99, 132)',
//                borderColor: 'rgb(255, 99, 132)',
//                borderWidth: 1
//            }]
//        },
//        options: {
//            scales: {
//                y: {
//                    beginAtZero: true
//                }
//            }
//        }
//    });

//}
  