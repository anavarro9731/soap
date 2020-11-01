import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { bus } from '../../modules/src/soap';
import * as S from './style';

const LoadingOverlay = props => {
  const numberOfRequestsToWaitFor = props.requests.length;

  const [loadingCount, setLoadingCount] = useState(numberOfRequestsToWaitFor);

  useEffect(() => {
    if (props.requests) {
      props.requests.forEach(request =>
        bus.subscribe({
          channel: bus.channels.queries,
          schema: request.schema,
          callback: () => setLoadingCount(loadingCount - 1),
        }),
      );
    }
  }, []);

  const showLoading = loadingCount > 0;

  return (
    <React.Fragment>
      {showLoading && <S.LoadingIcon />}
      <S.HideChildren hide={showLoading}>{props.children}</S.HideChildren>
    </React.Fragment>
  );
};

LoadingOverlay.propTypes = {
  children: PropTypes.node,
  requests: PropTypes.array,
};

LoadingOverlay.defaultProps = {
  requests: [],
};

export default LoadingOverlay;
