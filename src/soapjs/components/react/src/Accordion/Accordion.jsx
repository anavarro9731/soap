import React, { useState } from 'react';
import PropTypes from 'prop-types';
import Section from '../Section';
import ChevronDown from '../../modules/style/images/chevron-down-solid.svg';
import ChevronRight from '../../modules/style/images/chevron-right-solid.svg';
import * as S from './style';

const Accordion = props => {
  const [sectionState, setSectionState] = useState({});

  const toggleExpanded = sectionId => {
    setSectionState({
      ...sectionState,
      [sectionId]: !sectionState[sectionId],
    });
  };

  return Object.values(props.sections).map((section, index) => {
    const sectionExpanded = sectionState[section.id] || false;
    const onClick = () => toggleExpanded(section.id);

    const lastItem = index === props.sections.length - 1;
    return (
      <S.AccordionItem key={section.id} withBorder={!lastItem}>
        <S.Heading>
          <S.ClickToToggleExpand onClick={onClick}>
            <S.Chevron src={sectionExpanded ? ChevronDown : ChevronRight} />
            <S.Title>{section.title}</S.Title>
          </S.ClickToToggleExpand>
          <S.RightAlignedContent>
            {section.rightAlignedContent}
          </S.RightAlignedContent>
        </S.Heading>
        <S.Content>{sectionExpanded && section.content}</S.Content>
      </S.AccordionItem>
    );
  });
};

Accordion.propTypes = {
  sections: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.string.isRequired,
      title: PropTypes.string,
      content: PropTypes.node,
      rightAlignedContent: PropTypes.node,
    }),
  ).isRequired,
};

export default Accordion;
