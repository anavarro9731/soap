import React, { useState } from 'react';
import PropTypes from 'prop-types';
import ChevronDown from '../../modules/src/style/images/chevron-down-solid.svg';
import ChevronRight from '../../modules/src/style/images/chevron-right-solid.svg';
import * as S from './style';

const Section = props => {
  const [isSectionExpanded, setSectionExpanded] = useState(props.startExpanded);

  const toggleExpanded = () => setSectionExpanded(!isSectionExpanded);

  return (
    <S.Section>
      <S.Heading>
        <S.ClickToToggleExpand onClick={() => toggleExpanded()}>
          <S.Chevron src={isSectionExpanded ? ChevronDown : ChevronRight} />
          <S.Title>{props.title}</S.Title>
        </S.ClickToToggleExpand>
        <S.RightAlignedContent>
          {props.rightAlignedContent}
        </S.RightAlignedContent>
      </S.Heading>
      <S.Content isSectionExpanded={isSectionExpanded}>
        {isSectionExpanded && props.children}
      </S.Content>
    </S.Section>
  );
};

Section.propTypes = {
  title: PropTypes.string,
  children: PropTypes.node,
  rightAlignedContent: PropTypes.node,
  startExpanded: PropTypes.bool,
};

Section.defaultProps = {
  startExpanded: false,
};

export default Section;
