﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Beetle.Server {

    public class BeforeQueryExecuteEventArgs : EventArgs {
        private readonly ActionContext _actionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeQueryExecuteEventArgs" /> class.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="query">The query.</param>
        public BeforeQueryExecuteEventArgs(ActionContext actionContext, IQueryable query) {
            _actionContext = actionContext;
            Query = query;
        }

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public ActionContext ActionContext {
            get { return _actionContext; }
        }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public IQueryable Query { get; set; }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <value>
        /// The user data.
        /// </value>
        public string UserData { get; set; }
    }

    public delegate void BeforeQueryExecuteDelegate(object sender, BeforeQueryExecuteEventArgs eventArgs);

    public class AfterQueryExecuteEventArgs : EventArgs {
        private readonly ActionContext _actionContext;
        private readonly IQueryable _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfterQueryExecuteEventArgs" /> class.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="userData">The user data.</param>
        public AfterQueryExecuteEventArgs(ActionContext actionContext, IQueryable query, object result, object userData) {
            _actionContext = actionContext;
            _query = query;
            Result = result;
            UserData = userData;
        }

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public ActionContext ActionContext {
            get { return _actionContext; }
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public IQueryable Query {
            get { return _query; }
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; set; }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <value>
        /// The user data.
        /// </value>
        public object UserData { get; set; }
    }

    public delegate void AfterQueryExecuteDelegate(object sender, AfterQueryExecuteEventArgs eventArgs);

    public class BeforeSaveEventArgs : EventArgs {
        private readonly IEnumerable<EntityBag> _entities;
        private readonly SaveContext _saveContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeSaveEventArgs" /> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="saveContext">The save context.</param>
        public BeforeSaveEventArgs(IEnumerable<EntityBag> entities, SaveContext saveContext) {
            _entities = entities;
            _saveContext = saveContext;
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEnumerable<EntityBag> Entities {
            get { return _entities; }
        }

        /// <summary>
        /// Gets the save context.
        /// </summary>
        /// <value>
        /// The save context.
        /// </value>
        public SaveContext SaveContext { get { return _saveContext; } }
    }

    public delegate void BeforeSaveDelegate(object sender, BeforeSaveEventArgs eventArgs);

    public class AfterSaveEventArgs : EventArgs {
        private readonly IEnumerable<EntityBag> _entities;
        private readonly SaveResult _saveResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfterSaveEventArgs" /> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="saveResult">The save result.</param>
        public AfterSaveEventArgs(IEnumerable<EntityBag> entities, SaveResult saveResult) {
            _entities = entities;
            _saveResult = saveResult;
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public IEnumerable<EntityBag> Entities {
            get { return _entities; }
        }

        /// <summary>
        /// Gets the save result.
        /// </summary>
        /// <value>
        /// The save result.
        /// </value>
        public SaveResult SaveResult {
            get { return _saveResult; }
        }
    }

    public delegate void AfterSaveDelegate(object sender, AfterSaveEventArgs eventArgs);
}
