import React from 'react';
import ReactDOM from 'react-dom';
import Table from './Table';
import AppWrapper from '../AppWrapper';

const data = [
  {
    label: 'abc',
    printer: 'Printer 1',
    paymentTerminalId: 'paymentTerminalId1',
  },
  {
    label: 'def',
    printer: 'Printer 2',
    paymentTerminalId: 'paymentTerminalId2',
  },
];

const columns = [
  {
    columnId: '1',
    header: 'Kiosk ID',
    render: data => data.label,
    width: '10%',
  },
  {
    columnId: '2',
    header: 'Printer',
    render: data => data.printer,
    width: 'auto',
  },
  {
    columnId: '3',
    header: 'Payment Information',
    render: data => data.paymentTerminalId,
  },
  {
    columnId: '4',
    header: 'Buttons',
    render: () => <input type="button" text="Button" />,
    width: '150px',
  },
];

ReactDOM.render(
  <AppWrapper useDefaultFont>
    <div style={{ padding: '10px', height: '90%', width: '74%' }}>
      <Table columns={columns} data={data} />
    </div>
  </AppWrapper>,
  document.getElementById('content'),
);
