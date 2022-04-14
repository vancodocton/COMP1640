////const labels = [
////    'Academic Department',
////    'Support Department'];
const data = {
    labels: ['Orange', 'Blue'],
    datasets: [
        {
            label: 'Dataset 1',
            data: [10, 20],
            backgroundColor: 'rgb(255, 99, 132)'
        }
    ]
};
const config = {
    type: 'pie',
    data: data,
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'Chart.js Pie Chart'
            }
        }
    },
};
const pieChart = new Chart(
    document.getElementById('pieChart'),
    config);