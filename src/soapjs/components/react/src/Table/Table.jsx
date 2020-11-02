import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';
import {
  defaultTextColour,
  defaultLightBackgroundColour,
  defaultBorderColour,
} from '../style/defaults';

const mapColumnsToHeaders = props =>
  props.columns.map(column => (
    <S.Cell
      key={column.columnId}
      textColour={props.headerTextColour}
      borderColour={props.tableBorderColour}
      backgroundColour={props.headerBackgroundColour}
      fontWeight="bold"
    >
      <S.CellContent padding={props.headerCellPadding}>
        {column.header}
      </S.CellContent>
    </S.Cell>
  ));

const mapDataToRows = props =>
  props.data.map(dataRow =>
    props.columns.map(column => (
      <S.Cell
        key={column.columnId}
        textColour={props.tableTextColour}
        borderColour={props.tableBorderColour}
        backgroundColour={props.tableBackgroundColour}
      >
        <S.CellContent padding={props.cellPadding}>
          {column.render(dataRow)}
        </S.CellContent>
      </S.Cell>
    )),
  );

const Table = props => {
  const headers = mapColumnsToHeaders(props);

  const rows = mapDataToRows(props);

  const columnWidths = props.columns.reduce(
    (widths, column) => `${widths} ${column.width || '1fr'}`,
    '',
  );

  return (
    <S.Table columnWidths={columnWidths}>
      {headers}
      {rows}
    </S.Table>
  );
};

Table.propTypes = {
  data: PropTypes.array,
  columns: PropTypes.arrayOf(
    PropTypes.shape({
      columnId: PropTypes.string.isRequired,
      header: PropTypes.string,
      render: PropTypes.func,
      width: PropTypes.string,
    }),
  ).isRequired,
  headerTextColour: PropTypes.string,
  headerBackgroundColour: PropTypes.string,
  tableTextColour: PropTypes.string,
  tableBackgroundColour: PropTypes.string,
  tableBorderColour: PropTypes.string,
  headerCellPadding: PropTypes.string,
  cellPadding: PropTypes.string,
};

Table.defaultProps = {
  headerTextColour: defaultTextColour,
  headerBackgroundColour: defaultLightBackgroundColour,
  tableTextColour: defaultTextColour,
  tableBackgroundColour: defaultLightBackgroundColour,
  tableBorderColour: defaultBorderColour,
  headerCellPadding: '10px',
  cellPadding: '8px',
};

export default Table;
