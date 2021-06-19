


function DrawChart() {
    var ctx = document.getElementById('myChart').getContext('2d');

    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: '',
                data: [],
                backgroundColor: 'rgb(255, 99, 132)',
                borderColor: 'rgb(255, 99, 132)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

}



function AddData(datatodisplay, labelstodisplay) {
    var ctx = document.getElementById('myChart').getContext('2d');


    ctx.myChart.data.labels = labelstodisplay;
    ctx.myChart.data.datasets[0].data[2] = datatodisplay;

    ctx.myChart.update();

    //var myChart = new Chart(ctx, {
    //    type: 'line',
    //    data: {
    //        labels: labelstodisplay,
    //        datasets: [{
    //            label: 'Number of Views',
    //            data: datatodisplay,
    //            backgroundColor: 'rgb(255, 99, 132)',
    //            borderColor: 'rgb(255, 99, 132)',
    //            borderWidth: 1
    //        }]
    //    },
    //    options: {
    //        scales: {
    //            y: {
    //                beginAtZero: true
    //            }
    //        }
    //    }
    //});

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
  