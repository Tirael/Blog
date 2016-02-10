namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    using Dixin.Common;
    using Dixin.Linq.Fundamentals;

    public static class TableHelper
    {
        public static void SetForeignKey<TOther>(
            this EntityRef<TOther> entityRef, Func<bool> areEqual, Action setKey)
            where TOther : class
        {
            if (!areEqual())
            {
                if (entityRef.HasLoadedOrAssignedValue)
                {
                    throw new ForeignKeyReferenceAlreadyHasValueException();
                }

                setKey();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        public static void Associate<TThis, TOther>(
            this TThis @this,
            Action setThisKey,
            ref EntityRef<TOther> thisEntityRef,
            TOther other,
            Func<TOther, EntitySet<TThis>> getOtherEntitySet)
            where TOther : class
            where TThis : class
        {
            TOther previousOther = thisEntityRef.Entity;
            if (previousOther != other || !thisEntityRef.HasLoadedOrAssignedValue)
            {
                if (previousOther != null)
                {
                    thisEntityRef.Entity = null;
                    getOtherEntitySet(previousOther).Remove(@this);
                }

                thisEntityRef.Entity = other;
                if (other != null)
                {
                    getOtherEntitySet(other).Add(@this);
                }
                setThisKey();
            }
        }

        public static TEntity Find<TEntity>(this DataContext database, params object[] keys)
            where TEntity : class
        {
            MetaType metaType = database.Mapping.GetMetaType(typeof(TEntity));
            Argument.Requires(metaType != null, $"{nameof(TEntity)} must be mapped.", nameof(TEntity));
            MetaDataMember[] primaryKeys = metaType
                .Select(type => type.DataMembers)
                .Where(member => member.IsPrimaryKey)
                .ToArray();
            Argument.Requires(
                keys.Length == primaryKeys.Length, $"{nameof(keys)} must have correctnumer of values.", nameof(keys));

            ParameterExpression entity = Expression.Parameter(typeof(TEntity), nameof(entity));
            Expression predicateBody = primaryKeys
                .Select((primaryKey, index) => Expression.Equal(
                    Expression.Property(entity, primaryKey.Name),
                    Expression.Constant(keys[index])))
                .Aggregate<Expression, Expression>(Expression.Constant(true), Expression.AndAlso);
            return database.GetTable<TEntity>().Single(Expression.Lambda<Func<TEntity, bool>>(predicateBody, entity));
        }
    }
}