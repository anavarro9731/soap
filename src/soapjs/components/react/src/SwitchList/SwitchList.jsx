import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { sortArrayByObjectPropertyAlphanumerically } from '@soap/modules/lib/utils/sorting';
import Checkbox from '../Checkbox';
import Button from '../Button';
import MultiLineSelect from '../MultiLineSelect';
import * as S from './style';

const getSelectedOptionsFromSelectRef = selectRef =>
  Array.prototype.map.call(selectRef.current.selectedOptions, option => ({
    value: option.value,
    label: option.label,
  }));

const removeSelectedOptionsFromList = (list, selectedOptions) => {
  const selectedOptionValues = selectedOptions.map(option => option.value);

  return list.filter(
    listItem => !selectedOptionValues.includes(listItem.value),
  );
};

const addSelectedOptionsToList = (list, selectedOptions) => {
  const newList = [...list, ...selectedOptions];
  return sortArrayByObjectPropertyAlphanumerically(
    newList,
    option => option.value,
  );
};

const SwitchList = props => {
  const [firstListOptions, setFirstListOptions] = useState([]);
  const [secondListOptions, setSecondListOptions] = useState([]);

  useEffect(() => {
    setFirstListOptions(props.initialFirstListOptions);
    setSecondListOptions(props.initialSecondListOptions);
  }, [props.firstListOptions, props.secondListOptions]);

  const handleRightArrowClick = () => {
    const selectedOptions = getSelectedOptionsFromSelectRef(props.firstListRef);
    setFirstListOptions(
      removeSelectedOptionsFromList(firstListOptions, selectedOptions),
    );
    setSecondListOptions(
      addSelectedOptionsToList(secondListOptions, selectedOptions),
    );
  };

  const handleLeftArrowClick = () => {
    const selectedOptions = getSelectedOptionsFromSelectRef(
      props.secondListRef,
    );
    setFirstListOptions(
      addSelectedOptionsToList(firstListOptions, selectedOptions),
    );
    setSecondListOptions(
      removeSelectedOptionsFromList(secondListOptions, selectedOptions),
    );
  };

  const handleCheckboxSelect = checkboxValue => {
    setCheckboxValue(checkboxValue);
    if (checkboxValue === true) {
      setSecondListOptions([...firstListOptions, ...secondListOptions]);
      setFirstListOptions([]);
    }
  };

  const [checkboxValue, setCheckboxValue] = useState(
    props.initialFirstListOptions.length === 0,
  );

  const disabled = props.withSelectAllCheckbox && checkboxValue;

  const commonListProps = {
    visibleOptions: props.visibleOptionsInEachList,
    width: '100%',
    height: props.listHeight,
    disabled,
  };
  const commonButtonProps = {
    width: 'fit-content',
    height: 'fit-content',
    fontSize: '28px',
    disabled,
  };

  const CustomButton = props.buttonComponent;
  const CustomCheckbox = props.checkboxComponent;

  return (
    <S.SwitchListVerticalMargin>
      {props.withSelectAllCheckbox && (
        <CustomCheckbox
          value={checkboxValue}
          onChange={handleCheckboxSelect}
          checkboxLabel={props.selectAllCheckboxLabel}
        />
      )}
      <S.Lists listWidth={props.listWidth}>
        <S.ListTitle>{props.firstListTitle}</S.ListTitle>
        <div />
        <S.ListTitle>{props.secondListTitle}</S.ListTitle>

        <MultiLineSelect
          options={firstListOptions}
          listRef={props.firstListRef}
          {...commonListProps}
        />
        <S.ArrowButtons>
          <CustomButton onClick={handleRightArrowClick} {...commonButtonProps}>
            &#8250;
          </CustomButton>
          <CustomButton onClick={handleLeftArrowClick} {...commonButtonProps}>
            &#8249;
          </CustomButton>
        </S.ArrowButtons>
        <MultiLineSelect
          options={secondListOptions}
          listRef={props.secondListRef}
          {...commonListProps}
        />
      </S.Lists>
    </S.SwitchListVerticalMargin>
  );
};

SwitchList.propTypes = {
  visibleOptionsInEachList: PropTypes.number,
  initialFirstListOptions: PropTypes.arrayOf(
    PropTypes.shape({
      value: PropTypes.string.isRequired,
      label: PropTypes.string.isRequired,
    }),
  ).isRequired,
  initialSecondListOptions: PropTypes.arrayOf(
    PropTypes.shape({
      value: PropTypes.string.isRequired,
      label: PropTypes.string.isRequired,
    }),
  ).isRequired,
  listWidth: PropTypes.string,
  buttonComponent: PropTypes.func,
  checkboxComponent: PropTypes.func,
  firstListTitle: PropTypes.string,
  secondListTitle: PropTypes.string,
  withSelectAllCheckbox: PropTypes.bool,
  selectAllCheckboxLabel: PropTypes.string,
  listHeight: PropTypes.string,
  firstListRef: PropTypes.object,
  secondListRef: PropTypes.object,
};

SwitchList.defaultProps = {
  visibleOptionsInEachList: 15,
  listWidth: 'auto',
  buttonComponent: Button,
  checkboxComponent: Checkbox,
  withSelectAllCheckbox: false,
  listHeight: '305px',
};

export default SwitchList;
